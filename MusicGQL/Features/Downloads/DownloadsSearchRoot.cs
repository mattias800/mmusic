using MusicGQL.Features.Downloads.Services;

namespace MusicGQL.Features.Downloads;

public class DownloadsSearchRoot
{
    public DownloadQueueState DownloadQueue(DownloadQueueService queue) => queue.Snapshot();

    public DownloadProgress? CurrentDownload(CurrentDownloadStateService state) => state.Get();
}


