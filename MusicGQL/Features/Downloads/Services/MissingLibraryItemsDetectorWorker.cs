using Microsoft.Extensions.Options;
using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.Downloads.Services;

public class MissingLibraryItemsDetectorOptions
{
    public bool Enabled { get; set; } = true;
    public int TargetMinQueueLength { get; set; } = 10;
    public int MaxBatchSize { get; set; } = 10;
    public int ScanIntervalSeconds { get; set; } = 120;
    public int CooldownHoursForRecentFailures { get; set; } = 24;
}

public class MissingLibraryItemsDetectorWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<MissingLibraryItemsDetectorWorker> logger,
    IOptions<MissingLibraryItemsDetectorOptions>? options
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("MissingLibraryItemsDetectorWorker started");
        var opts = options?.Value ?? new MissingLibraryItemsDetectorOptions();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!opts.Enabled)
                {
                    await Task.Delay(TimeSpan.FromSeconds(opts.ScanIntervalSeconds), stoppingToken);
                    continue;
                }

                using var scope = scopeFactory.CreateScope();
                var queue = scope.ServiceProvider.GetRequiredService<DownloadQueueService>();
                var cache = scope.ServiceProvider.GetRequiredService<ServerLibraryCache>();
                var history = scope.ServiceProvider.GetRequiredService<DownloadHistoryService>();

                var snapshot = queue.Snapshot();
                var deficit = Math.Max(0, opts.TargetMinQueueLength - snapshot.QueueLength);
                logger.LogDebug(
                    "[MissingDetector] QueueLength={QueueLength}, targetMin={Target}, deficit={Deficit}",
                    snapshot.QueueLength,
                    opts.TargetMinQueueLength,
                    deficit
                );
                if (deficit <= 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(opts.ScanIntervalSeconds), stoppingToken);
                    continue;
                }

                // Build candidate list of missing releases = releases with zero available tracks
                var allReleases = await cache.GetAllReleasesAsync();
                var candidates = allReleases
                    .Where(r =>
                        (r.Tracks?.Count ?? 0) > 0
                        && r.Tracks.All(t =>
                            t.CachedMediaAvailabilityStatus
                            != CachedMediaAvailabilityStatus.Available
                        )
                    )
                    .Select(r => new
                    {
                        r.ArtistId,
                        r.FolderName,
                        r.JsonRelease.Type,
                    })
                    .Distinct()
                    .ToList();
                logger.LogDebug(
                    "[MissingDetector] Found {Count} missing releases before filtering",
                    candidates.Count
                );

                // Exclude items currently in queue
                var inQueue = snapshot
                    .Items.Select(i => ($"{i.ArtistId}|{i.ReleaseFolderName}"))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                candidates.RemoveAll(c => inQueue.Contains($"{c.ArtistId}|{c.FolderName}"));
                logger.LogDebug(
                    "[MissingDetector] After excluding in-queue, {Count} candidates remain",
                    candidates.Count
                );

                // Exclude recent failures (cooldown)
                var cutoff = DateTime.UtcNow.AddHours(-opts.CooldownHoursForRecentFailures);
                var recentlyFailed = history
                    .List()
                    .Where(h => !h.Success && h.TimestampUtc >= cutoff)
                    .Select(h => ($"{h.ArtistId}|{h.ReleaseFolderName}"))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                candidates.RemoveAll(c => recentlyFailed.Contains($"{c.ArtistId}|{c.FolderName}"));
                logger.LogDebug(
                    "[MissingDetector] After excluding recent failures, {Count} candidates remain",
                    candidates.Count
                );

                if (candidates.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(opts.ScanIntervalSeconds), stoppingToken);
                    continue;
                }

                // Shuffle candidates
                // Prioritize by type: Album > Ep > Single, then shuffle within each priority
                int Priority(ServerLibrary.Json.JsonReleaseType t) =>
                    t switch
                    {
                        ServerLibrary.Json.JsonReleaseType.Album => 0,
                        ServerLibrary.Json.JsonReleaseType.Ep => 1,
                        ServerLibrary.Json.JsonReleaseType.Single => 2,
                        _ => 3,
                    };
                var rng = new Random();
                candidates = candidates
                    .GroupBy(c => Priority(c.Type))
                    .OrderBy(g => g.Key)
                    .SelectMany(g => g.OrderBy(_ => rng.Next()))
                    .ToList();

                var take = Math.Min(opts.MaxBatchSize, deficit);
                var toEnqueue = candidates
                    .Take(take)
                    .Select(c => new DownloadQueueItem(c.ArtistId, c.FolderName))
                    .ToList();
                if (toEnqueue.Count > 0)
                {
                    logger.LogInformation(
                        "[MissingDetector] Enqueuing {Count} releases to top up queue",
                        toEnqueue.Count
                    );
                    queue.Enqueue(toEnqueue);
                }
                else
                {
                    logger.LogInformation(
                        "[MissingDetector] Nothing to enqueue (candidates={Candidates}, deficit={Deficit})",
                        candidates.Count,
                        deficit
                    );
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MissingLibraryItemsDetectorWorker error");
            }
            finally
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(opts.ScanIntervalSeconds), stoppingToken);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}
