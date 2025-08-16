namespace MusicGQL.Features.Downloads.Services;

public interface IDownloadSlotManager
{
    IReadOnlyDictionary<int, DownloadSlot> Slots { get; }
    int QueueLength { get; }
    int ActiveSlotCount { get; }
    
    Task<bool> EnqueueWorkAsync(DownloadQueueItem item, CancellationToken cancellationToken);
    bool TryDequeue(out DownloadQueueItem? item);
    Task UpdateSlotConfigurationAsync(int newSlotCount, CancellationToken cancellationToken);
    Task StopAllSlotsAsync(CancellationToken cancellationToken);
    List<DownloadQueueItem> GetQueueSnapshot();
    List<DownloadSlotInfo> GetSlotsInfo();
}
