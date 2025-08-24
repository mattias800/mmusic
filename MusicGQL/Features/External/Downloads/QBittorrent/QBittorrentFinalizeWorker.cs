using Microsoft.Extensions.Options;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerSettings;

namespace MusicGQL.Features.External.Downloads.QBittorrent;

public class QBittorrentFinalizeWorkerOptions
{
    public bool Enabled { get; set; } = true;
    public int ScanIntervalMinutes { get; set; } = 5;
    public int MaxReleasesPerScan { get; set; } = 50;
    public int AttemptCooldownMinutes { get; set; } = 10;
}

public class QBittorrentFinalizeWorker(
    IOptions<QBittorrentFinalizeWorkerOptions> options,
    ILogger<QBittorrentFinalizeWorker> logger,
    ServerLibraryCache cache,
    IQBittorrentFinalizeService qbFinalize,
    MusicGQL.Features.Downloads.Services.DownloadLogPathProvider logPathProvider
) : BackgroundService
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, DateTime> _lastAttempt = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opts = options?.Value ?? new QBittorrentFinalizeWorkerOptions();
        if (!opts.Enabled)
        {
            logger.LogInformation("[qB Finalize Worker] Service disabled");
            return;
        }

        logger.LogInformation("[qB Finalize Worker] Started. Interval={Interval} min, MaxReleasesPerScan={Max}", opts.ScanIntervalMinutes, opts.MaxReleasesPerScan);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ScanOnceAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(opts.ScanIntervalMinutes), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[qB Finalize Worker] Error during scan");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    internal async Task<int> ScanOnceAsync(CancellationToken cancellationToken)
    {
        var opts = options?.Value ?? new QBittorrentFinalizeWorkerOptions();
        var now = DateTime.UtcNow;
        int attempts = 0;

        // Get missing releases
        var allReleases = await cache.GetAllReleasesAsync();
        var missing = allReleases
            .Where(r => (r.Tracks?.Count ?? 0) > 0 && r.Tracks.All(t => t.CachedMediaAvailabilityStatus != CachedMediaAvailabilityStatus.Available))
            .Take(opts.MaxReleasesPerScan)
            .ToList();

        if (missing.Count == 0)
        {
            logger.LogDebug("[qB Finalize Worker] No missing releases found");
            return 0;
        }

        foreach (var rel in missing)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var key = $"{rel.ArtistId}|{rel.FolderName}".ToLowerInvariant();
            if (_lastAttempt.TryGetValue(key, out var last) && last > now.AddMinutes(-opts.AttemptCooldownMinutes))
            {
                continue; // cooldown
            }

            _lastAttempt.AddOrUpdate(key, now, (_, _) => now);
            attempts++;

            // Write a small breadcrumb into per-release log
            try
            {
                var relLogPath = await logPathProvider.GetReleaseLogFilePathAsync(rel.ArtistName ?? rel.ArtistId, rel.Title ?? rel.FolderName, cancellationToken);
                if (!string.IsNullOrWhiteSpace(relLogPath))
                {
                    var rlog = new MusicGQL.Features.Downloads.Services.DownloadLogger(relLogPath!);
                    rlog.Info("[qB Finalize Worker] Attempting finalize via worker");
                }
            }
            catch { }

            try
            {
                var ok = await qbFinalize.FinalizeReleaseAsync(rel.ArtistId, rel.FolderName, cancellationToken);
                if (ok)
                {
                    logger.LogInformation("[qB Finalize Worker] Finalize succeeded for {Artist}/{Release}", rel.ArtistId, rel.FolderName);
                }
                else
                {
                    logger.LogDebug("[qB Finalize Worker] Finalize skipped or not complete for {Artist}/{Release}", rel.ArtistId, rel.FolderName);
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "[qB Finalize Worker] Finalize attempt failed for {Artist}/{Release}", rel.ArtistId, rel.FolderName);
            }
        }

        if (attempts > 0)
        {
            logger.LogInformation("[qB Finalize Worker] Attempted finalize for {Count} releases", attempts);
        }
        return attempts;
    }
}
