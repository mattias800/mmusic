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
    private readonly ConcurrentDictionary<string, byte> _dedupeKeys = new(StringComparer.OrdinalIgnoreCase);

    private static string BuildDedupeKey(ArtistImportQueueItem item)
    {
        // Compose a key that captures the job identity; keep simple but collision-resistant enough
        // Import jobs: artist name + mbid + ext id + song title
        // Refresh jobs: local artist id + release folder
        var key = item.JobKind switch
        {
            ArtistImportJobKind.RefreshReleaseMetadata => $"refresh|{item.LocalArtistId}|{item.ReleaseFolderName}",
            _ => $"import|{item.ArtistName}|{item.MusicBrainzArtistId}|{item.ExternalArtistId}|{item.SongTitle}"
        };
        return key;
    }

    public void Enqueue(IEnumerable<ArtistImportQueueItem> items)
    {
        int count = 0;
        var preview = new List<string>();
        foreach (var item in items)
        {
            var key = BuildDedupeKey(item);
            if (_dedupeKeys.TryAdd(key, 1))
            {
                _queue.Enqueue(item with { QueueKey = key });
                count++;
                if (preview.Count < 10)
                {
                    preview.Add($"{item.ArtistName} (job={item.JobKind}, song='{item.SongTitle ?? ""}', extId='{item.ExternalArtistId ?? ""}', mb='{item.MusicBrainzArtistId ?? ""}')");
                }
            }
            else
            {
                logger.LogInformation("Skipped duplicate job: {Key}", key);
            }
        }
        logger.LogInformation("Enqueued {Count} artist imports. First items: {Preview}", count, string.Join(", ", preview));
        PublishQueueUpdated();
    }

    public void Enqueue(ArtistImportQueueItem item)
    {
        var key = BuildDedupeKey(item);
        if (_dedupeKeys.TryAdd(key, 1))
        {
            _queue.Enqueue(item with { QueueKey = key });
            logger.LogInformation(
                "Enqueued job: {Artist} (job={Job}, song='{Song}', extId='{Ext}', mb='{Mb}')",
                item.ArtistName,
                item.JobKind,
                item.SongTitle ?? string.Empty,
                item.ExternalArtistId ?? string.Empty,
                item.MusicBrainzArtistId ?? string.Empty
            );
        }
        else
        {
            logger.LogInformation("Skipped duplicate job: {Key}", key);
        }
        PublishQueueUpdated();
    }

    public bool TryDequeue(out ArtistImportQueueItem? item)
    {
        var ok = _queue.TryDequeue(out var dequeued);
        item = dequeued;
        if (ok)
        {
            // Remove dedupe key now that we are going to process it
            try
            {
                var key = BuildDedupeKey(dequeued!);
                _dedupeKeys.TryRemove(key, out _);
            }
            catch { }
            logger.LogInformation(
                "Dequeued job: {Artist} (job={Job}, song='{Song}', extId='{Ext}', mb='{Mb}'). Remaining queue length: {Len}",
                dequeued!.ArtistName,
                dequeued!.JobKind,
                dequeued!.SongTitle ?? string.Empty,
                dequeued!.ExternalArtistId ?? string.Empty,
                dequeued!.MusicBrainzArtistId ?? string.Empty,
                _queue.Count
            );
            PublishQueueUpdated();
        }
        return ok;
    }

    public bool TryRemove(string queueKey)
    {
        var removed = false;
        var list = _queue.ToArray().ToList();
        _queue.Clear();
        foreach (var q in list)
        {
            if (!removed && string.Equals(q.QueueKey, queueKey, StringComparison.Ordinal))
            {
                removed = true;
                try { _dedupeKeys.TryRemove(BuildDedupeKey(q), out _); } catch { }
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


