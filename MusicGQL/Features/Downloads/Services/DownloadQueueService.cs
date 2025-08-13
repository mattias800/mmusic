using System.Collections.Concurrent;
using HotChocolate.Subscriptions;

namespace MusicGQL.Features.Downloads.Services;

public class DownloadQueueService(
    ITopicEventSender eventSender,
    ILogger<DownloadQueueService> logger
)
{
    private readonly ConcurrentQueue<DownloadQueueItem> _queue = new();
    private readonly ConcurrentQueue<DownloadQueueItem> _priorityQueue = new();
    private readonly ConcurrentDictionary<string, byte> _dedupeKeys = new(StringComparer.OrdinalIgnoreCase);

    private static string BuildKey(DownloadQueueItem item)
    {
        return $"dl|{item.ArtistId}|{item.ReleaseFolderName}";
    }

    public void Enqueue(IEnumerable<DownloadQueueItem> items)
    {
        int count = 0;
        foreach (var item in items)
        {
            var key = BuildKey(item);
            if (_dedupeKeys.TryAdd(key, 1))
            {
                _queue.Enqueue(item with { QueueKey = key });
                count++;
            }
        }
        logger.LogInformation("[DownloadQueue] Enqueued {Count} releases", count);
        PublishQueueUpdated();
    }

    public void Enqueue(DownloadQueueItem item)
    {
        var key = BuildKey(item);
        if (_dedupeKeys.TryAdd(key, 1))
        {
            _queue.Enqueue(item with { QueueKey = key });
            logger.LogInformation("[DownloadQueue] Enqueued 1 release ({ArtistId}/{Folder})", item.ArtistId, item.ReleaseFolderName);
        }
        PublishQueueUpdated();
    }

    public void EnqueueFront(IEnumerable<DownloadQueueItem> items)
    {
        int count = 0;
        foreach (var item in items)
        {
            var key = BuildKey(item);
            if (_dedupeKeys.TryAdd(key, 1))
            {
                _priorityQueue.Enqueue(item with { QueueKey = key });
                count++;
            }
        }
        logger.LogInformation("[DownloadQueue] Enqueued {Count} releases at FRONT (priority)", count);
        PublishQueueUpdated();
    }

    public void EnqueueFront(DownloadQueueItem item)
    {
        var key = BuildKey(item);
        if (_dedupeKeys.TryAdd(key, 1))
        {
            _priorityQueue.Enqueue(item with { QueueKey = key });
            logger.LogInformation("[DownloadQueue] Enqueued 1 release at FRONT (priority) ({ArtistId}/{Folder})", item.ArtistId, item.ReleaseFolderName);
        }
        PublishQueueUpdated();
    }

    public bool TryDequeue(out DownloadQueueItem? item)
    {
        // Always drain priority queue first
        var ok = _priorityQueue.TryDequeue(out var dequeued);
        if (!ok)
        {
            ok = _queue.TryDequeue(out dequeued);
        }
        item = dequeued;
        if (ok)
        {
            try { _dedupeKeys.TryRemove(BuildKey(dequeued!), out _); } catch { }
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
                try { _dedupeKeys.TryRemove(BuildKey(q), out _); } catch { }
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
                    try { _dedupeKeys.TryRemove(BuildKey(q), out _); } catch { }
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
        var pri = _priorityQueue.ToArray();
        var norm = _queue.ToArray();
        var items = pri.Concat(norm).ToArray();
        return new DownloadQueueState
        {
            Id = "downloadQueue",
            QueueLength = items.Length,
            Items = items.Take(25).ToList(),
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
                try { _dedupeKeys.TryRemove(BuildKey(q), out _); } catch { }
                continue;
            }
            _priorityQueue.Enqueue(q);
        }
        foreach (var q in norm)
        {
            if (string.Equals(q.ArtistId, artistId, StringComparison.OrdinalIgnoreCase))
            {
                removed++;
                try { _dedupeKeys.TryRemove(BuildKey(q), out _); } catch { }
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

    private void PublishQueueUpdated()
    {
        _ = eventSender.SendAsync(
            DownloadsSubscription.DownloadQueueUpdatedTopic,
            Snapshot()
        );
    }
}


