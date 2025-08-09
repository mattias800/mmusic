using MusicGQL.Features.Downloads;

namespace MusicGQL.Features.ServerLibrary.Cache;

public enum CachedReleaseDownloadStatus
{
    DownloadButtonVisible,
    Searching,
    Downloading,
    DownloadButtonNotVisible,
}

public static class CachedReleaseDownloadStatusExtensions
{
    public static ReleaseDownloadStatus ToGql(this CachedReleaseDownloadStatus status) =>
        status switch
        {
            CachedReleaseDownloadStatus.DownloadButtonVisible =>
                ReleaseDownloadStatus.DownloadButtonVisible,
            CachedReleaseDownloadStatus.Searching => ReleaseDownloadStatus.Searching,
            CachedReleaseDownloadStatus.Downloading => ReleaseDownloadStatus.Downloading,
            _ => ReleaseDownloadStatus.DownloadButtonNotVisible,
        };
}
