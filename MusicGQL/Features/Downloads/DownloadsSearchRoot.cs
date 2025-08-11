using MusicGQL.Features.Downloads.Services;

namespace MusicGQL.Features.Downloads;

public class DownloadsSearchRoot
{
    public Downloads.DownloadQueueState DownloadQueue(DownloadQueueService queue) => queue.Snapshot();

    public Downloads.DownloadProgress? CurrentDownload(CurrentDownloadStateService state) => state.Get();
}


