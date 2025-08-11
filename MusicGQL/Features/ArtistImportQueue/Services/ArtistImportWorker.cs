using MusicGQL.Features.Import;
using MusicGQL.Features.ServerLibrary.Utils;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.ArtistImportQueue.Services;

public class ArtistImportWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<ArtistImportWorker> logger
) : BackgroundService
{
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

                // Resolve MBID (try by name; if SongTitle provided, bias search by recording)
                string? mbArtistId = null;
                try
                {
                    if (!string.IsNullOrWhiteSpace(item.SongTitle))
                    {
                        var recs = await mb.SearchRecordingByNameAsync(item.SongTitle!);
                        var artists = recs.SelectMany(r => r.Credits?.Select(c => c.Artist)?.Where(a => a != null) ?? []).ToList();
                        var match = artists.FirstOrDefault(a => string.Equals(a!.Name, item.ArtistName, StringComparison.OrdinalIgnoreCase));
                        mbArtistId = match?.Id;
                    }

                    if (string.IsNullOrWhiteSpace(mbArtistId))
                    {
                        var candidates = await mb.SearchArtistByNameAsync(item.ArtistName, 10, 0);
                        mbArtistId = candidates.FirstOrDefault()?.Id;
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


