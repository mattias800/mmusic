using HotChocolate.Subscriptions;
using System.Collections.Concurrent;

namespace MusicGQL.Features.Downloads.Services;

public class CurrentDownloadStateService(
    ITopicEventSender eventSender,
    ILogger<CurrentDownloadStateService> logger
)
{
    // Internal state for backward compatibility (not exposed via GraphQL)
    private DownloadProgress? _legacyState = null;
    
    // New multi-slot state
    private readonly ConcurrentDictionary<int, DownloadProgress> _slotProgress = new();

    // Legacy methods for internal use only (not exposed via GraphQL)
    public DownloadProgress? Get() => _legacyState;

    public void Reset()
    {
        _legacyState = null;
    }

    public void Set(DownloadProgress progress)
    {
        _legacyState = progress;
    }

    public void SetStatus(DownloadStatus status)
    {
        _legacyState = (_legacyState ?? new DownloadProgress()) with { Status = status };
    }

    public void SetTrackProgress(int completed, int total)
    {
        _legacyState = (_legacyState ?? new DownloadProgress()) with { CompletedTracks = completed, TotalTracks = total };
    }

    public void SetError(string error)
    {
        _legacyState = (_legacyState ?? new DownloadProgress()) with { Status = DownloadStatus.Failed, ErrorMessage = error };
    }

    // New multi-slot methods (exposed via GraphQL)
    public IReadOnlyDictionary<int, DownloadProgress> GetAllSlotProgress() => _slotProgress.AsReadOnly();

    public DownloadProgress? GetSlotProgress(int slotId)
    {
        _slotProgress.TryGetValue(slotId, out var progress);
        return progress;
    }

    public async Task UpdateSlotProgressAsync(int slotId, DownloadProgress progress, CancellationToken cancellationToken)
    {
        _slotProgress[slotId] = progress;
        await PublishSlotProgressAsync(slotId, progress, cancellationToken);
    }

    public async Task ClearSlotProgressAsync(int slotId, CancellationToken cancellationToken)
    {
        if (_slotProgress.TryRemove(slotId, out var _))
        {
            await PublishSlotProgressAsync(slotId, null, cancellationToken);
        }
    }

    public void UpdateSlotStatus(int slotId, DownloadStatus status)
    {
        if (_slotProgress.TryGetValue(slotId, out var existing))
        {
            var updated = existing with { Status = status };
            _slotProgress[slotId] = updated;
            _ = PublishSlotProgressAsync(slotId, updated, CancellationToken.None);
        }
    }

    public void UpdateSlotTrackProgress(int slotId, int completed, int total)
    {
        if (_slotProgress.TryGetValue(slotId, out var existing))
        {
            var updated = existing with { CompletedTracks = completed, TotalTracks = total };
            _slotProgress[slotId] = updated;
            _ = PublishSlotProgressAsync(slotId, updated, CancellationToken.None);
        }
    }

    public void UpdateSlotProvider(int slotId, string? provider, int providerIndex, int totalProviders)
    {
        if (_slotProgress.TryGetValue(slotId, out var existing))
        {
            var updated = existing with 
            { 
                CurrentProvider = provider, 
                CurrentProviderIndex = providerIndex, 
                TotalProviders = totalProviders 
            };
            _slotProgress[slotId] = updated;
            _ = PublishSlotProgressAsync(slotId, updated, CancellationToken.None);
        }
    }

    public void UpdateSlotError(int slotId, string error)
    {
        if (_slotProgress.TryGetValue(slotId, out var existing))
        {
            var updated = existing with { Status = DownloadStatus.Failed, ErrorMessage = error };
            _slotProgress[slotId] = updated;
            _ = PublishSlotProgressAsync(slotId, updated, CancellationToken.None);
        }
    }

    private async Task PublishSlotProgressAsync(int slotId, DownloadProgress? progress, CancellationToken cancellationToken)
    {
        try
        {
            await eventSender.SendAsync(
                DownloadsSubscription.SlotProgressUpdatedTopic,
                new SlotProgressUpdate(slotId, progress),
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to publish slot progress update for slot {SlotId}", slotId);
        }
    }
}

public record SlotProgressUpdate(int SlotId, DownloadProgress? Progress);


