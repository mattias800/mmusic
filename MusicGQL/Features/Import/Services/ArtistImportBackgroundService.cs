using HotChocolate.Subscriptions;
using MusicGQL.Features.ArtistImportQueue.Services;
using MusicGQL.Features.Downloads;
using MusicGQL.Features.Downloads.Services;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Subscription;
using MusicGQL.Features.ServerLibrary.Writer;
using MusicGQL.Features.ServerSettings;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import.Services;

/// <summary>
/// Background service for handling full artist import process asynchronously
/// </summary>
public class ArtistImportBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ArtistImportBackgroundService> logger,
    ITopicEventSender eventSender
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ArtistImportBackgroundService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var queue =
                    scope.ServiceProvider.GetRequiredService<ArtistImportBackgroundQueueService>();

                if (!queue.TryDequeue(out var job) || job is null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    continue;
                }

                logger.LogInformation(
                    "[ArtistImportBackground] Starting background import for artist: {ArtistName} (MBID: {MusicBrainzId})",
                    job.ArtistName,
                    job.MusicBrainzId
                );

                await ProcessArtistImportJobAsync(job, scope.ServiceProvider, stoppingToken);

                logger.LogInformation(
                    "[ArtistImportBackground] Completed background import for artist: {ArtistName}",
                    job.ArtistName
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ArtistImportBackgroundService main loop");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ProcessArtistImportJobAsync(
        ArtistImportBackgroundJob job,
        IServiceProvider services,
        CancellationToken cancellationToken
    )
    {
        var cache = services.GetRequiredService<ServerLibraryCache>();
        var writer = services.GetRequiredService<ServerLibraryJsonWriter>();
        var mbImport = services.GetRequiredService<MusicBrainzImportService>();
        var importExecutor = services.GetRequiredService<IImportExecutor>();
        var enrichmentService = services.GetRequiredService<LastFmEnrichmentService>();
        var downloadQueue = services.GetRequiredService<DownloadQueueService>();
        var settingsAccessor = services.GetRequiredService<ServerSettingsAccessor>();

        try
        {
            // Step 1: Fetch artist metadata from MusicBrainz
            await PublishProgressAsync(
                job.ArtistId,
                "Fetching artist metadata from MusicBrainz...",
                10
            );
            var mbArtist = await mbImport.GetArtistByIdAsync(job.MusicBrainzId);
            if (mbArtist == null)
            {
                await PublishErrorAsync(job.ArtistId, "Failed to fetch artist from MusicBrainz");
                return;
            }

            // Step 2: Download photos and create basic artist.json
            await PublishProgressAsync(job.ArtistId, "Downloading artist photos...", 20);
            var artistDir = job.ArtistPath;
            await importExecutor.ImportOrEnrichArtistAsync(
                artistDir,
                job.MusicBrainzId,
                mbArtist.Name
            );

            // Step 3: Fetch top tracks
            await PublishProgressAsync(job.ArtistId, "Fetching top tracks...", 30);
            try
            {
                await enrichmentService.EnrichArtistAsync(artistDir, job.MusicBrainzId);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "[ArtistImportBackground] Artist enrichment failed, continuing without enrichment"
                );
            }

            // Step 4: Get list of all releases
            await PublishProgressAsync(job.ArtistId, "Fetching release information...", 40);
            var releaseGroups = await mbImport.GetArtistReleaseGroupsAsync(job.MusicBrainzId);
            var eligibleReleases = releaseGroups
                .Where(rg =>
                    !(
                        rg.SecondaryTypes?.Any(t =>
                            t.Equals("Demo", StringComparison.OrdinalIgnoreCase)
                        ) ?? false
                    )
                )
                .ToList();

            await PublishProgressAsync(
                job.ArtistId,
                $"Found {eligibleReleases.Count} releases to process",
                50
            );

            // Step 5: Import release metadata (without audio)
            await PublishProgressAsync(job.ArtistId, "Importing release metadata...", 60);
            var importedCount = await importExecutor.ImportEligibleReleaseGroupsAsync(
                artistDir,
                job.MusicBrainzId
            );

            await PublishProgressAsync(job.ArtistId, $"Imported {importedCount} releases", 70);

            // Step 6: Update cache
            await PublishProgressAsync(job.ArtistId, "Updating library cache...", 80);
            await cache.UpdateCacheAsync();

            // Step 7: Enqueue releases for download
            await PublishProgressAsync(job.ArtistId, "Enqueuing releases for download...", 90);
            var artist = await cache.GetArtistByIdAsync(job.ArtistId);
            if (artist?.Releases != null)
            {
                var downloadJobs = artist
                    .Releases.Select(r => new DownloadQueueItem(r.ArtistId, r.FolderName))
                    .ToList();

                foreach (var downloadJob in downloadJobs)
                {
                    downloadQueue.Enqueue(downloadJob);
                }

                await PublishProgressAsync(
                    job.ArtistId,
                    $"Enqueued {downloadJobs.Count} releases for download",
                    100
                );
            }

            // Step 8: Publish final artist update
            var finalArtist = await cache.GetArtistByIdAsync(job.ArtistId);
            if (finalArtist != null)
            {
                await eventSender.SendAsync(
                    LibrarySubscription.LibraryArtistUpdatedTopic(job.ArtistId),
                    new Artists.Artist(finalArtist)
                );

                await PublishProgressAsync(
                    job.ArtistId,
                    "Artist import completed successfully!",
                    100
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[ArtistImportBackground] Failed to process artist import job for {ArtistId}",
                job.ArtistId
            );
            await PublishErrorAsync(job.ArtistId, $"Import failed: {ex.Message}");
        }
    }

    private async Task PublishProgressAsync(string artistId, string message, int percentage)
    {
        try
        {
            var progress = new ArtistImportBackgroundProgress(
                artistId,
                message,
                percentage,
                DateTime.UtcNow
            );

            await eventSender.SendAsync($"ArtistImportBackgroundProgress_{artistId}", progress);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to publish artist import progress for {ArtistId}",
                artistId
            );
        }
    }

    private async Task PublishErrorAsync(string artistId, string error)
    {
        try
        {
            var progress = new ArtistImportBackgroundProgress(
                artistId,
                error,
                0,
                DateTime.UtcNow,
                true
            );

            await eventSender.SendAsync($"ArtistImportBackgroundProgress_{artistId}", progress);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to publish artist import error for {ArtistId}", artistId);
        }
    }
}

/// <summary>
/// Job definition for background artist import
/// </summary>
public record ArtistImportBackgroundJob(
    string ArtistId,
    string ArtistName,
    string MusicBrainzId,
    string ArtistPath
);

/// <summary>
/// Progress update for background artist import
/// </summary>
public record ArtistImportBackgroundProgress(
    string ArtistId,
    string Message,
    int Percentage,
    DateTime Timestamp,
    bool HasError = false
);
