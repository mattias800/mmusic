using System.Collections.Concurrent;
using HotChocolate.Subscriptions;

namespace MusicGQL.Features.Downloads.Services;

public class DownloadQueueService(
    ITopicEventSender eventSender,
    ILogger<DownloadQueueService> logger
)
{
    private readonly ConcurrentQueue<DownloadQueueItem> _queue = new();
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
        }
        PublishQueueUpdated();
    }

    public bool TryDequeue(out DownloadQueueItem? item)
    {
        var ok = _queue.TryDequeue(out var dequeued);
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
        // Non-blocking remove by rebuilding queue without the matching key
        var removed = false;
        var list = _queue.ToArray().ToList();
        _queue.Clear();
        foreach (var q in list)
        {
            if (!removed && string.Equals(q.QueueKey, queueKey, StringComparison.Ordinal))
            {
                removed = true;
                try { _dedupeKeys.TryRemove(BuildKey(q), out _); } catch { }
                continue;
            }
            _queue.Enqueue(q);
        }
        if (removed)
        {
            PublishQueueUpdated();
        }
        return removed;
    }

    public DownloadQueueState Snapshot()
    {
        var items = _queue.ToArray();
        return new DownloadQueueState
        {
            Id = "downloadQueue",
            QueueLength = items.Length,
            Items = items.Take(25).ToList(),
        };
    }

    private void PublishQueueUpdated()
    {
        _ = eventSender.SendAsync(
            DownloadsSubscription.DownloadQueueUpdatedTopic,
            Snapshot()
        );
    }
}


