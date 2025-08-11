using MusicGQL.Features.Import;
using MusicGQL.Features.ServerLibrary.Utils;
using MusicGQL.Integration.MusicBrainz;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Artists;
using MusicGQL.Features.Playlists.Subscription;

namespace MusicGQL.Features.ArtistImportQueue.Services;

public class ArtistImportWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<ArtistImportWorker> logger
) : BackgroundService
{
    private static string NormalizeArtistName(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var s = input
            .Replace("’", "'")
            .Replace("“", "\"")
            .Replace("”", "\"");
        // Normalize ampersand vs 'and' to a single comparable form
        s = s.Replace("&", " and ");
        // Collapse whitespace and lowercase
        var normalized = System.Text.RegularExpressions.Regex.Replace(s, "\\s+", " ")
            .Trim()
            .ToLowerInvariant();
        return normalized;
    }

    private static string SwapAmpersandAnd(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return name;
        if (name.Contains('&'))
        {
            return System.Text.RegularExpressions.Regex.Replace(name, "\\s*&\\s*", " and ", System.Text.RegularExpressions.RegexOptions.CultureInvariant);
        }
        if (System.Text.RegularExpressions.Regex.IsMatch(name, @"\\band\\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            return System.Text.RegularExpressions.Regex.Replace(name, @"\\band\\b", "&", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        return name;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Artist import worker started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var queue = scope.ServiceProvider.GetRequiredService<ArtistImportQueueService>();
                var progress = scope.ServiceProvider.GetRequiredService<CurrentArtistImportStateService>();
                var mb = scope.ServiceProvider.GetRequiredService<MusicBrainzService>();
                var importer = scope.ServiceProvider.GetRequiredService<LibraryImportService>();
                var cache = scope.ServiceProvider.GetRequiredService<ServerLibrary.Cache.ServerLibraryCache>();
                var events = scope.ServiceProvider.GetRequiredService<HotChocolate.Subscriptions.ITopicEventSender>();
                var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

                if (!queue.TryDequeue(out var item) || item is null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    continue;
                }

                progress.Set(new ArtistImportProgress
                {
                    ArtistName = item.ArtistName,
                    SongTitle = item.SongTitle,
                    Status = ArtistImportStatus.ResolvingArtist,
                    TotalReleases = 0,
                    CompletedReleases = 0,
                });

                // Resolve MBID (prefer provided, else try by name; if SongTitle provided, bias search by recording)
                string? mbArtistId = item.MusicBrainzArtistId ?? item.ExternalArtistId;
                try
                {
                    if (string.IsNullOrWhiteSpace(mbArtistId) && !string.IsNullOrWhiteSpace(item.SongTitle))
                    {
                        var recs = await mb.SearchRecordingByNameAsync(item.SongTitle!);
                        var artists = recs.SelectMany(r => r.Credits?.Select(c => c.Artist)?.Where(a => a != null) ?? []).ToList();
                        var targetNorm = NormalizeArtistName(item.ArtistName);
                        var match = artists.FirstOrDefault(a => NormalizeArtistName(a!.Name) == targetNorm);
                        mbArtistId = match?.Id;
                    }

                    if (string.IsNullOrWhiteSpace(mbArtistId))
                    {
                        var candidates = await mb.SearchArtistByNameAsync(item.ArtistName, 10, 0);
                        var targetNorm = NormalizeArtistName(item.ArtistName);
                        var exact = candidates.FirstOrDefault(c => NormalizeArtistName(c.Name) == targetNorm);
                        mbArtistId = exact?.Id ?? candidates.FirstOrDefault()?.Id;

                        // Fallback: try swapping '&' and 'and'
                        if (string.IsNullOrWhiteSpace(mbArtistId))
                        {
                            var alt = SwapAmpersandAnd(item.ArtistName);
                            if (!string.Equals(alt, item.ArtistName, StringComparison.OrdinalIgnoreCase))
                            {
                                var altCandidates = await mb.SearchArtistByNameAsync(alt, 10, 0);
                                var altExact = altCandidates.FirstOrDefault(c => NormalizeArtistName(c.Name) == targetNorm);
                                mbArtistId = altExact?.Id ?? altCandidates.FirstOrDefault()?.Id;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to resolve MusicBrainz artist for {Artist}", item.ArtistName);
                }

                if (string.IsNullOrWhiteSpace(mbArtistId))
                {
                    progress.Set(new ArtistImportProgress
                    {
                        ArtistName = item.ArtistName,
                        SongTitle = item.SongTitle,
                        Status = ArtistImportStatus.Failed,
                        ErrorMessage = "Artist not found on MusicBrainz",
                    });
                    continue;
                }

                // Pre-compute number of eligible releases for progress
                int totalEligible = 0;
                try
                {
                    var rgs = await mb.GetReleaseGroupsForArtistAsync(mbArtistId);
                    totalEligible = rgs.Count(rg => LibraryDecider.ShouldBeAddedWhenAddingArtistToServerLibrary(rg));
                }
                catch { }

                progress.Set(new ArtistImportProgress
                {
                    ArtistName = item.ArtistName,
                    SongTitle = item.SongTitle,
                    MusicBrainzArtistId = mbArtistId,
                    Status = ArtistImportStatus.ImportingArtist,
                    TotalReleases = totalEligible,
                    CompletedReleases = 0,
                });

                try
                {
                    var res = await importer.ImportArtistByMusicBrainzIdAsync(mbArtistId);
                    if (!string.IsNullOrWhiteSpace(res.ErrorMessage) || res.Success == false)
                    {
                        progress.Set(new ArtistImportProgress
                        {
                            ArtistName = item.ArtistName,
                            SongTitle = item.SongTitle,
                            MusicBrainzArtistId = mbArtistId,
                            Status = ArtistImportStatus.Failed,
                            ErrorMessage = res.ErrorMessage ?? "Unknown error",
                        });
                    }
                    else
                    {
                        progress.Set(new ArtistImportProgress
                        {
                            ArtistName = item.ArtistName,
                            SongTitle = item.SongTitle,
                            MusicBrainzArtistId = mbArtistId,
                            Status = ArtistImportStatus.Completed,
                            TotalReleases = totalEligible,
                            CompletedReleases = totalEligible,
                        });

                        // Publish the newly imported Artist object
                        try
                        {
                            await cache.UpdateCacheAsync();
                            var importedArtist = await cache.GetArtistByNameAsync(item.ArtistName);
                            if (importedArtist != null)
                            {
                                await events.SendAsync(
                                    ArtistImportSubscription.ArtistImportedTopic,
                                    new Artist(importedArtist)
                                );

                                // Centralized artist-updated publication
                                await events.SendAsync(
                                    ServerLibrary.Subscription.LibrarySubscription.LibraryArtistUpdatedTopic(importedArtist.Id),
                                    new Artist(importedArtist)
                                );

                                // Prefer updating by external artist id when available; fallback to name match
                                if (!string.IsNullOrWhiteSpace(item.ExternalArtistId))
                                {
                                    var playlists = await db
                                        .Playlists.Include(p => p.Items)
                                        .Where(p => p.Items.Any(i => i.LocalArtistId == null && i.ExternalArtistId == item.ExternalArtistId))
                                        .ToListAsync();

                                    foreach (var pl in playlists)
                                    {
                                        foreach (var it in pl.Items.Where(i => i.LocalArtistId == null && i.ExternalArtistId == item.ExternalArtistId))
                                        {
                                            it.LocalArtistId = importedArtist.Id;
                                            await events.SendAsync(
                                                PlaylistSubscription.PlaylistItemUpdatedTopic(pl.Id),
                                                new PlaylistSubscription.PlaylistItemUpdatedMessage(pl.Id, it.Id)
                                            );
                                        }
                                    }
                                }

                                // If this queue item was enqueued from a specific playlist item and it still isn't linked,
                                // link it now by LocalArtistId
                                if (!string.IsNullOrWhiteSpace(item.PlaylistId) && !string.IsNullOrWhiteSpace(item.PlaylistItemId))
                                {
                                    var playlist = await db
                                        .Playlists.Include(p => p.Items)
                                        .FirstOrDefaultAsync(p => p.Id == item.PlaylistId);
                                    var pi = playlist?.Items.FirstOrDefault(i => i.Id == item.PlaylistItemId);
                                    if (pi is not null && string.IsNullOrWhiteSpace(pi.LocalArtistId))
                                    {
                                        pi.LocalArtistId = importedArtist.Id;
                                        await events.SendAsync(
                                            PlaylistSubscription.PlaylistItemUpdatedTopic(playlist!.Id),
                                            new PlaylistSubscription.PlaylistItemUpdatedMessage(playlist!.Id, pi.Id)
                                        );
                                        await db.SaveChangesAsync();
                                    }
                                }
                                // If there is no external artist id on the queue item, we intentionally do not update
                                // by name to avoid collisions between different artists with identical names.

                                await db.SaveChangesAsync();
                            }
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error importing artist {Artist}", item.ArtistName);
                    progress.Set(new ArtistImportProgress
                    {
                        ArtistName = item.ArtistName,
                        SongTitle = item.SongTitle,
                        MusicBrainzArtistId = mbArtistId,
                        Status = ArtistImportStatus.Failed,
                        ErrorMessage = ex.Message,
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Artist import worker loop error");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}


