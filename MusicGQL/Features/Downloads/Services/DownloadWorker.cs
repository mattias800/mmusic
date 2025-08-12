using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.Downloads.Services;

public class DownloadWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<DownloadWorker> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Download worker started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var queue = scope.ServiceProvider.GetRequiredService<DownloadQueueService>();
                var starter = scope.ServiceProvider.GetRequiredService<StartDownloadReleaseService>();
                var cache = scope.ServiceProvider.GetRequiredService<ServerLibraryCache>();

                if (!queue.TryDequeue(out var job) || job is null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    continue;
                }

                // Start the download for this queued release
                try
                {
                    await starter.StartAsync(job.ArtistId, job.ReleaseFolderName, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "[DownloadWorker] Error starting job {Artist}/{Folder}", job.ArtistId, job.ReleaseFolderName);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Download worker loop error");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
