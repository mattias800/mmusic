using System.Collections.Concurrent;
using HotChocolate.Subscriptions;

namespace MusicGQL.Features.Import.Services;

/// <summary>
/// Queue service for managing background artist import jobs
/// </summary>
public class ArtistImportBackgroundQueueService(
    ITopicEventSender eventSender,
    ILogger<ArtistImportBackgroundQueueService> logger
)
{
    private readonly ConcurrentQueue<ArtistImportBackgroundJob> _queue = new();
    private readonly ConcurrentDictionary<string, byte> _dedupeKeys = new(
        StringComparer.OrdinalIgnoreCase
    );

    private static string BuildDedupeKey(ArtistImportBackgroundJob job)
    {
        return $"artist_import|{job.MusicBrainzId}";
    }

    /// <summary>
    /// Enqueues a new artist import job
    /// </summary>
    public void Enqueue(ArtistImportBackgroundJob job)
    {
        var key = BuildDedupeKey(job);
        if (_dedupeKeys.TryAdd(key, 1))
        {
            _queue.Enqueue(job);
            logger.LogInformation(
                "[ArtistImportBackgroundQueue] Enqueued artist import job for {ArtistName} (MBID: {MusicBrainzId})",
                job.ArtistName,
                job.MusicBrainzId
            );
            PublishQueueUpdated();
        }
        else
        {
            logger.LogDebug(
                "[ArtistImportBackgroundQueue] Artist import job already queued for {ArtistName} (MBID: {MusicBrainzId})",
                job.ArtistName,
                job.MusicBrainzId
            );
        }
    }

    /// <summary>
    /// Tries to dequeue the next job
    /// </summary>
    public bool TryDequeue(out ArtistImportBackgroundJob? job)
    {
        var ok = _queue.TryDequeue(out var dequeued);
        job = dequeued;

        if (ok && dequeued != null)
        {
            // Remove dedupe key now that we are going to process it
            try
            {
                var key = BuildDedupeKey(dequeued);
                _dedupeKeys.TryRemove(key, out _);
            }
            catch { }

            logger.LogInformation(
                "[ArtistImportBackgroundQueue] Dequeued artist import job for {ArtistName} (MBID: {MusicBrainzId}). Remaining queue length: {QueueLength}",
                dequeued.ArtistName,
                dequeued.MusicBrainzId,
                _queue.Count
            );
            PublishQueueUpdated();
        }

        return ok;
    }

    /// <summary>
    /// Gets the current queue state
    /// </summary>
    public ArtistImportBackgroundQueueState Snapshot()
    {
        var items = _queue.ToArray();
        return new ArtistImportBackgroundQueueState
        {
            QueueLength = items.Length,
            Items = items.Take(25).ToList() ?? [], // Limit to first 25 for performance
        };
    }

    /// <summary>
    /// Gets the current queue length
    /// </summary>
    public int QueueLength => _queue.Count;

    private void PublishQueueUpdated()
    {
        _ = eventSender.SendAsync("ArtistImportBackgroundQueueUpdated", Snapshot());
    }
}

/// <summary>
/// State snapshot of the background artist import queue
/// </summary>
public record ArtistImportBackgroundQueueState(
    int QueueLength = 0,
    List<ArtistImportBackgroundJob> Items = null
)
{
    public List<ArtistImportBackgroundJob> Items { get; init; } = Items ?? [];
};
