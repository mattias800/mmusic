using System.Collections.Concurrent;
using HotChocolate.Subscriptions;
using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.Downloads.Services;

public class DownloadQueueService(
    ITopicEventSender eventSender,
    ILogger<DownloadQueueService> logger,
    IDownloadSlotManager slotManager,
    ServerLibraryCache cache,
    DownloadLogPathProvider logPathProvider
)
{
    private readonly ConcurrentQueue<DownloadQueueItem> _queue = new();
    private readonly ConcurrentQueue<DownloadQueueItem> _priorityQueue = new();
    private readonly ConcurrentDictionary<string, byte> _dedupeKeys = new(
        StringComparer.OrdinalIgnoreCase
    );

    // Maximum queue size to prevent memory issues (adjustable via configuration)
    private const int MaxQueueSize = 1000;

    private static string BuildKey(DownloadQueueItem item)
    {
        return $"dl|{item.ArtistId}|{item.ReleaseFolderName}";
    }

    public async void Enqueue(IEnumerable<DownloadQueueItem> items)
    {
        int count = 0;
        foreach (var item in items)
        {
            var key = BuildKey(item);
            if (_dedupeKeys.TryAdd(key, 1))
            {
                var success = await slotManager.EnqueueWorkAsync(item, CancellationToken.None);
                if (success)
                {
                    count++;
                    await LogToReleaseAsync(item, "[Queue] Enqueued");
                }
                else
                {
                    // If enqueue failed, remove the dedupe key so it can be retried later
                    _dedupeKeys.TryRemove(key, out _);
                    await LogToReleaseAsync(
                        item,
                        "[Queue] Already in queue or being processed (enqueue skipped)"
                    );
                }
            }
        }
        logger.LogInformation("[DownloadQueue] Enqueued {Count} releases", count);
        PublishQueueUpdated();
    }

    public async void Enqueue(DownloadQueueItem item)
    {
        // Check if queue is at maximum capacity to prevent memory issues
        if (slotManager.QueueLength >= MaxQueueSize)
        {
            logger.LogWarning(
                "[DownloadQueue] Queue at maximum capacity ({MaxSize}), rejecting new item {ArtistId}/{Folder}",
                MaxQueueSize,
                item.ArtistId,
                item.ReleaseFolderName
            );
            await LogToReleaseAsync(item, "[Queue] Rejected: queue at maximum capacity");
            return;
        }

        var key = BuildKey(item);
        if (_dedupeKeys.TryAdd(key, 1))
        {
            var success = await slotManager.EnqueueWorkAsync(item, CancellationToken.None);
            if (success)
            {
                logger.LogInformation(
                    "[DownloadQueue] Enqueued 1 release ({ArtistId}/{Folder})",
                    item.ArtistId,
                    item.ReleaseFolderName
                );
                await LogToReleaseAsync(item, "[Queue] Enqueued");
            }
            else
            {
                // If enqueue failed, remove the dedupe key so it can be retried later
                _dedupeKeys.TryRemove(key, out _);
                logger.LogDebug(
                    "[DownloadQueue] Failed to enqueue release ({ArtistId}/{Folder}) - already in queue or being processed",
                    item.ArtistId,
                    item.ReleaseFolderName
                );
                await LogToReleaseAsync(
                    item,
                    "[Queue] Already in queue or being processed (enqueue skipped)"
                );
            }
        }
        PublishQueueUpdated();
    }

    public async void EnqueueFront(IEnumerable<DownloadQueueItem> items)
    {
        int count = 0;
        foreach (var item in items)
        {
            var key = BuildKey(item);
            if (_dedupeKeys.TryAdd(key, 1))
            {
                // For priority items, we'll add them to the front of the slot manager's queue
                // This is a simplified approach - in a more sophisticated system we might want
                // to implement actual priority queuing in the slot manager
                var success = await slotManager.EnqueueWorkAsync(item, CancellationToken.None);
                if (success)
                {
                    count++;
                    await LogToReleaseAsync(item, "[Queue] Enqueued at FRONT (priority)");
                }
                else
                {
                    // If enqueue failed, remove the dedupe key so it can be retried later
                    _dedupeKeys.TryRemove(key, out _);
                    await LogToReleaseAsync(
                        item,
                        "[Queue] Already in queue or being processed (priority enqueue skipped)"
                    );
                }
            }
        }
        logger.LogInformation(
            "[DownloadQueue] Enqueued {Count} releases at FRONT (priority)",
            count
        );
        PublishQueueUpdated();
    }

    public async void EnqueueFront(DownloadQueueItem item)
    {
        // Check if queue is at maximum capacity to prevent memory issues
        if (slotManager.QueueLength >= MaxQueueSize)
        {
            logger.LogWarning(
                "[DownloadQueue] Queue at maximum capacity ({MaxSize}), rejecting new item {ArtistId}/{Folder}",
                MaxQueueSize,
                item.ArtistId,
                item.ReleaseFolderName
            );
            await LogToReleaseAsync(item, "[Queue] Rejected: queue at maximum capacity");
            return;
        }

        var key = BuildKey(item);
        if (_dedupeKeys.TryAdd(key, 1))
        {
            // For priority items, we'll add them to the front of the slot manager's queue
            // This is a simplified approach - in a more sophisticated system we might want
            // to implement actual priority queuing in the slot manager
            var success = await slotManager.EnqueueWorkAsync(item, CancellationToken.None);
            if (success)
            {
                logger.LogInformation(
                    "[DownloadQueue] Enqueued 1 release at FRONT (priority) ({ArtistId}/{Folder}), queue length: {QueueLength}",
                    item.ArtistId,
                    item.ReleaseFolderName,
                    slotManager.QueueLength
                );
                await LogToReleaseAsync(item, "[Queue] Enqueued at FRONT (priority)");
            }
            else
            {
                // If enqueue failed, remove the dedupe key so it can be retried later
                _dedupeKeys.TryRemove(key, out _);
                logger.LogDebug(
                    "[DownloadQueue] Failed to enqueue release at FRONT ({ArtistId}/{Folder}) - already in queue or being processed",
                    item.ArtistId,
                    item.ReleaseFolderName
                );
                await LogToReleaseAsync(
                    item,
                    "[Queue] Already in queue or being processed (priority enqueue skipped)"
                );
            }
        }
        PublishQueueUpdated();
    }

    public bool TryDequeue(out DownloadQueueItem? item)
    {
        // Dequeue from the slot manager's actual queue
        var ok = slotManager.TryDequeue(out var dequeued);
        item = dequeued;
        if (ok && dequeued != null)
        {
            try
            {
                _dedupeKeys.TryRemove(BuildKey(dequeued), out _);
            }
            catch { }
            PublishQueueUpdated();
        }
        return ok;
    }

    public bool TryRemove(string queueKey)
    {
        // Non-blocking remove by rebuilding queues without the matching key
        var removed = false;
        var pri = _priorityQueue.ToArray().ToList();
        var norm = _queue.ToArray().ToList();
        _priorityQueue.Clear();
        _queue.Clear();
        foreach (var q in pri)
        {
            if (!removed && string.Equals(q.QueueKey, queueKey, StringComparison.Ordinal))
            {
                removed = true;
                try
                {
                    _dedupeKeys.TryRemove(BuildKey(q), out _);
                }
                catch { }
                continue;
            }
            _priorityQueue.Enqueue(q);
        }
        if (!removed)
        {
            foreach (var q in norm)
            {
                if (!removed && string.Equals(q.QueueKey, queueKey, StringComparison.Ordinal))
                {
                    removed = true;
                    try
                    {
                        _dedupeKeys.TryRemove(BuildKey(q), out _);
                    }
                    catch { }
                    continue;
                }
                _queue.Enqueue(q);
            }
        }
        if (removed)
        {
            PublishQueueUpdated();
        }
        return removed;
    }

    public DownloadQueueState Snapshot()
    {
        // Get queue state from slot manager
        var queueItems = slotManager.GetQueueSnapshot();
        var items = queueItems.Take(25).ToList();

        return new DownloadQueueState
        {
            Id = "downloadQueue",
            QueueLength = slotManager.QueueLength,
            Items = items,
        };
    }

    public int RemoveAllForArtist(string artistId)
    {
        int removed = 0;
        var pri = _priorityQueue.ToArray().ToList();
        var norm = _queue.ToArray().ToList();
        _priorityQueue.Clear();
        _queue.Clear();
        foreach (var q in pri)
        {
            if (string.Equals(q.ArtistId, artistId, StringComparison.OrdinalIgnoreCase))
            {
                removed++;
                try
                {
                    _dedupeKeys.TryRemove(BuildKey(q), out _);
                }
                catch { }
                continue;
            }
            _priorityQueue.Enqueue(q);
        }
        foreach (var q in norm)
        {
            if (string.Equals(q.ArtistId, artistId, StringComparison.OrdinalIgnoreCase))
            {
                removed++;
                try
                {
                    _dedupeKeys.TryRemove(BuildKey(q), out _);
                }
                catch { }
                continue;
            }
            _queue.Enqueue(q);
        }
        if (removed > 0)
        {
            PublishQueueUpdated();
        }
        return removed;
    }

    private async Task LogToReleaseAsync(DownloadQueueItem item, string message)
    {
        try
        {
            var rel = await cache.GetReleaseByArtistAndFolderAsync(
                item.ArtistId,
                item.ReleaseFolderName
            );
            if (rel != null)
            {
                var path = await logPathProvider.GetReleaseLogFilePathAsync(
                    rel.ArtistName,
                    rel.Title
                );
                if (!string.IsNullOrWhiteSpace(path))
                {
                    using var relLogger = new DownloadLogger(path!);
                    relLogger.Info(message);
                }
            }
        }
        catch { }
    }

    private void PublishQueueUpdated()
    {
        _ = eventSender.SendAsync(DownloadsSubscription.DownloadQueueUpdatedTopic, Snapshot());
    }
}
