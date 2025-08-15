namespace MusicGQL.Features.Downloads.Services;

public interface IDownloadSlotManager
{
    IReadOnlyDictionary<int, DownloadSlot> Slots { get; }
    int QueueLength { get; }
    int ActiveSlotCount { get; }
    
    Task EnqueueWorkAsync(DownloadQueueItem item, CancellationToken cancellationToken);
    Task UpdateSlotConfigurationAsync(int newSlotCount, CancellationToken cancellationToken);
    Task StopAllSlotsAsync(CancellationToken cancellationToken);
    List<DownloadQueueItem> GetQueueSnapshot();
    List<DownloadSlotInfo> GetSlotsInfo();
}
