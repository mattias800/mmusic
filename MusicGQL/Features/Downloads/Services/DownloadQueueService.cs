using System.Collections.Concurrent;
using HotChocolate.Subscriptions;

namespace MusicGQL.Features.Downloads.Services;

public class DownloadQueueService(
    ITopicEventSender eventSender,
    ILogger<DownloadQueueService> logger
)
{
    private readonly ConcurrentQueue<Downloads.DownloadQueueItem> _queue = new();

    public void Enqueue(IEnumerable<Downloads.DownloadQueueItem> items)
    {
        int count = 0;
        foreach (var item in items)
        {
            _queue.Enqueue(item);
            count++;
        }
        logger.LogInformation("[DownloadQueue] Enqueued {Count} releases", count);
        PublishQueueUpdated();
    }

    public void Enqueue(Downloads.DownloadQueueItem item)
    {
        _queue.Enqueue(item);
        PublishQueueUpdated();
    }

    public bool TryDequeue(out Downloads.DownloadQueueItem? item)
    {
        var ok = _queue.TryDequeue(out var dequeued);
        item = dequeued;
        if (ok)
        {
            PublishQueueUpdated();
        }
        return ok;
    }

    public Downloads.DownloadQueueState Snapshot()
    {
        var items = _queue.ToArray();
        return new Downloads.DownloadQueueState
        {
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


