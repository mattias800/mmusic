using System.Collections.Concurrent;
using System.Collections.Generic;
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
        var preview = new List<string>();
        foreach (var item in items)
        {
            _queue.Enqueue(item);
            count++;
            if (preview.Count < 10)
            {
                preview.Add($"{item.ArtistName} (song='{item.SongTitle ?? ""}', extId='{item.ExternalArtistId ?? ""}', mb='{item.MusicBrainzArtistId ?? ""}')");
            }
        }
        logger.LogInformation("Enqueued {Count} artist imports. First items: {Preview}", count, string.Join(", ", preview));
        PublishQueueUpdated();
    }

    public void Enqueue(ArtistImportQueueItem item)
    {
        _queue.Enqueue(item);
        logger.LogInformation(
            "Enqueued single artist import: {Artist} (song='{Song}', extId='{Ext}', mb='{Mb}')",
            item.ArtistName,
            item.SongTitle ?? string.Empty,
            item.ExternalArtistId ?? string.Empty,
            item.MusicBrainzArtistId ?? string.Empty
        );
        PublishQueueUpdated();
    }

    public bool TryDequeue(out ArtistImportQueueItem? item)
    {
        var ok = _queue.TryDequeue(out var dequeued);
        item = dequeued;
        if (ok)
        {
            logger.LogInformation(
                "Dequeued artist import: {Artist} (song='{Song}', extId='{Ext}', mb='{Mb}'). Remaining queue length: {Len}",
                dequeued!.ArtistName,
                dequeued!.SongTitle ?? string.Empty,
                dequeued!.ExternalArtistId ?? string.Empty,
                dequeued!.MusicBrainzArtistId ?? string.Empty,
                _queue.Count
            );
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


