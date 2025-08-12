using MusicGQL.Features.Import;
using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ServerLibrary.Utils;
using MusicGQL.Integration.MusicBrainz;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Artists;
using MusicGQL.Features.Playlists.Subscription;
using MusicGQL.Features.ServerLibrary;
using Path = System.IO.Path;

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

                if (item.JobKind == ArtistImportJobKind.RefreshReleaseMetadata &&
                    !string.IsNullOrWhiteSpace(item.LocalArtistId) &&
                    !string.IsNullOrWhiteSpace(item.ReleaseFolderName))
                {
                    // Handle release metadata refresh as part of the same worker
                    var artistId = item.LocalArtistId!;
                    var releaseFolderName = item.ReleaseFolderName!;
                    logger.LogInformation("[ArtistImportWorker] Refreshing release metadata for {Artist}/{Folder}", artistId, releaseFolderName);

                    var release = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);
                    if (release == null)
                    {
                        logger.LogWarning("[ArtistImportWorker] Release not found: {Artist}/{Folder}", artistId, releaseFolderName);
                        continue;
                    }
                    var artist = await cache.GetArtistByIdAsync(artistId);
                    var mbidForRefresh = artist?.JsonArtist.Connections?.MusicBrainzArtistId;
                    if (string.IsNullOrWhiteSpace(mbidForRefresh))
                    {
                        logger.LogWarning("[ArtistImportWorker] Artist missing MBID: {Artist}", artistId);
                        continue;
                    }

                    try
                    {
                        var artistFolderPath = Path.GetDirectoryName(release.ReleasePath) ?? Path.Combine("./Library", artistId);

                        string? releaseGroupId = release.JsonRelease.Connections?.MusicBrainzReleaseGroupId;
                        string? primaryType = null;

                        if (string.IsNullOrWhiteSpace(releaseGroupId))
                        {
                            var rgs = await mb.GetReleaseGroupsForArtistAsync(mbidForRefresh);
                            var match = rgs.FirstOrDefault(rg => string.Equals(rg.Title, release.Title, StringComparison.OrdinalIgnoreCase));
                            if (match == null)
                            {
                                logger.LogWarning("[ArtistImportWorker] No RG match for {Artist}/{Folder}", artistId, releaseFolderName);
                                continue;
                            }
                            releaseGroupId = match.Id;
                            primaryType = match.PrimaryType;
                        }

                        var releaseImporter = scope.ServiceProvider.GetRequiredService<LibraryReleaseImportService>();
                        if (!string.IsNullOrWhiteSpace(releaseGroupId))
                        {
                            await releaseImporter.ImportReleaseGroupInPlaceAsync(
                                releaseGroupId,
                                release.Title,
                                primaryType,
                                artistFolderPath,
                                artistId,
                                releaseFolderName
                            );
                        }

                        await cache.UpdateReleaseFromJsonAsync(artistId, releaseFolderName);
                        var updated = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);
                        if (updated != null)
                        {
                            await events.SendAsync(ServerLibrary.Subscription.LibrarySubscription.LibraryReleaseMetadataUpdatedTopic(artistId, releaseFolderName), new Release(updated));
                            await events.SendAsync(ServerLibrary.Subscription.LibrarySubscription.LibraryReleaseUpdatedTopic(artistId, releaseFolderName), new Release(updated));
                            await events.SendAsync(ServerLibrary.Subscription.LibrarySubscription.LibraryArtistReleaseUpdatedTopic(artistId), new Release(updated));
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "[ArtistImportWorker] Error refreshing {Artist}/{Folder}", artistId, releaseFolderName);
                    }

                    // go back to next queue item
                    continue;
                }

                logger.LogInformation(
                    "Processing artist import: {Artist} (song='{Song}', extId='{Ext}', mb='{Mb}')",
                    item.ArtistName,
                    item.SongTitle ?? string.Empty,
                    item.ExternalArtistId ?? string.Empty,
                    item.MusicBrainzArtistId ?? string.Empty
                );

                progress.Set(new ArtistImportProgress
                {
                    ArtistName = item.ArtistName,
                    SongTitle = item.SongTitle,
                    Status = ArtistImportStatus.ResolvingArtist,
                    TotalReleases = 0,
                    CompletedReleases = 0,
                });

                // Resolve MBID (prefer provided MusicBrainz id; otherwise try by song/artist name)
                string? mbArtistId = item.MusicBrainzArtistId;
                try
                {
                    if (string.IsNullOrWhiteSpace(mbArtistId) && !string.IsNullOrWhiteSpace(item.ExternalArtistId))
                    {
                        // ExternalArtistId is typically a Spotify artist id; do not treat it as a MusicBrainz id.
                        logger.LogDebug("External artist id present for '{Artist}' (extId='{ExtId}'). Will resolve MBID via search instead of using external id.", item.ArtistName, item.ExternalArtistId);
                    }

                    if (string.IsNullOrWhiteSpace(mbArtistId) && !string.IsNullOrWhiteSpace(item.SongTitle))
                    {
                        logger.LogDebug("Attempting to resolve MBID via recording search for song '{Song}'", item.SongTitle);
                        var recs = await mb.SearchRecordingByNameAsync(item.SongTitle!);
                        var artists = recs.SelectMany(r => r.Credits?.Select(c => c.Artist)?.Where(a => a != null) ?? []).ToList();
                        var targetNorm = NormalizeArtistName(item.ArtistName);
                        var match = artists.FirstOrDefault(a => NormalizeArtistName(a!.Name) == targetNorm);
                        mbArtistId = match?.Id;
                    }

                    if (string.IsNullOrWhiteSpace(mbArtistId))
                    {
                        logger.LogDebug("Attempting to resolve MBID via artist search for '{Artist}'", item.ArtistName);
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
                                logger.LogDebug("Retrying MBID resolution with alt name '{AltName}'", alt);
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
                    logger.LogWarning("Could not resolve MusicBrainz artist id for '{Artist}' - skipping", item.ArtistName);
                    progress.Set(new ArtistImportProgress
                    {
                        ArtistName = item.ArtistName,
                        SongTitle = item.SongTitle,
                        Status = ArtistImportStatus.Failed,
                        ErrorMessage = "Artist not found on MusicBrainz",
                    });
                    continue;
                }

                logger.LogInformation("Resolved MBID {MbArtistId} for artist '{Artist}'", mbArtistId, item.ArtistName);

                // Pre-compute number of eligible releases for progress
                int totalEligible = 0;
                try
                {
                    var rgs = await mb.GetReleaseGroupsForArtistAsync(mbArtistId);
                    totalEligible = rgs.Count(rg => LibraryDecider.ShouldBeAddedWhenAddingArtistToServerLibrary(rg));
                    logger.LogDebug("Computed {Count} eligible release groups for artist '{Artist}'", totalEligible, item.ArtistName);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to pre-compute eligible release groups for '{Artist}'", item.ArtistName);
                }

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
                        logger.LogError("Import failed for '{Artist}' (MBID {MbArtistId}): {Error}", item.ArtistName, mbArtistId, res.ErrorMessage ?? "Unknown error");
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
                        logger.LogInformation("Import completed for '{Artist}' (MBID {MbArtistId}). Updating cache and notifying subscribers.", item.ArtistName, mbArtistId);
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
                                logger.LogInformation("Imported artist now present in cache: {ArtistName} (Id {ArtistId})", importedArtist.Name, importedArtist.Id);
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
                                    logger.LogDebug("Attempting to link playlist items by external artist id {ExtId}", item.ExternalArtistId);
                                    var playlists = await db
                                        .Playlists.Include(p => p.Items)
                                        .Where(p => p.Items.Any(i => i.LocalArtistId == null && i.ExternalArtistId == item.ExternalArtistId))
                                        .ToListAsync();

                                    foreach (var pl in playlists)
                                    {
                                        foreach (var it in pl.Items.Where(i => i.LocalArtistId == null && i.ExternalArtistId == item.ExternalArtistId))
                                        {
                                            it.LocalArtistId = importedArtist.Id;
                                            logger.LogInformation("Linked playlist item {PlaylistItemId} in playlist {PlaylistId} to artist {ArtistId}", it.Id, pl.Id, importedArtist.Id);
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
                                        logger.LogInformation("Backfilled specific playlist item {PlaylistItemId} in playlist {PlaylistId} to artist {ArtistId}", pi.Id, playlist!.Id, importedArtist.Id);
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
                            else
                            {
                                logger.LogWarning("Artist '{Artist}' was imported but not found in cache immediately after update.", item.ArtistName);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error post-processing imported artist '{Artist}'", item.ArtistName);
                        }

                        // If queue is empty, clear the current progress state so UI shows Idle
                        try
                        {
                            if (!queue.TryDequeue(out var next) || next is null)
                            {
                                progress.Reset();
                            }
                            else
                            {
                                // If we dequeued something to check emptiness, put it back at front by re-enqueueing
                                queue.Enqueue(next);
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


