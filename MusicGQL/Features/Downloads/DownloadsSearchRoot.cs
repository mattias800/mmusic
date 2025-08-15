using MusicGQL.Features.Downloads.Services;

namespace MusicGQL.Features.Downloads;

public class DownloadsSearchRoot
{
    public DownloadQueueState DownloadQueue(DownloadQueueService queue) => queue.Snapshot();



    public List<DownloadHistoryItem> DownloadHistory(DownloadHistoryService history) => history.List();

    // New multi-slot queries
    public IReadOnlyDictionary<int, DownloadProgress> AllSlotProgress(CurrentDownloadStateService state) => state.GetAllSlotProgress();

    public DownloadProgress? SlotProgress(CurrentDownloadStateService state, int slotId) => state.GetSlotProgress(slotId);

    public List<DownloadSlotInfo> DownloadSlots(IDownloadSlotManager slotManager) => slotManager.GetSlotsInfo();
}


