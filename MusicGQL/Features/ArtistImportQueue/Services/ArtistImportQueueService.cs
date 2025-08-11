using System.Collections.Concurrent;
using HotChocolate.Subscriptions;

namespace MusicGQL.Features.ArtistImportQueue.Services;

public class ArtistImportQueueService(
    ITopicEventSender eventSender,
    ILogger<ArtistImportQueueService> logger
)
{
    private readonly ConcurrentQueue<ArtistImportQueueItem> _queue = new();

    public void Enqueue(IEnumerable<ArtistImportQueueItem> items)
    {
        int count = 0;
        foreach (var item in items)
        {
            _queue.Enqueue(item);
            count++;
        }
        logger.LogInformation("Enqueued {Count} artist imports", count);
        PublishQueueUpdated();
    }

    public void Enqueue(ArtistImportQueueItem item)
    {
        _queue.Enqueue(item);
        PublishQueueUpdated();
    }

    public bool TryDequeue(out ArtistImportQueueItem? item)
    {
        var ok = _queue.TryDequeue(out var dequeued);
        item = dequeued;
        if (ok)
        {
            PublishQueueUpdated();
        }
        return ok;
    }

    public ArtistImportQueueState Snapshot()
    {
        var items = _queue.ToArray();
        return new ArtistImportQueueState
        {
            QueueLength = items.Length,
            Items = items.Take(25).ToList(),
        };
    }

    private void PublishQueueUpdated()
    {
        _ = eventSender.SendAsync(
            ArtistImportSubscription.ArtistImportQueueUpdatedTopic,
            Snapshot()
        );
    }
}


