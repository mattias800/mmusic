using MusicGQL.Features.Downloads;

namespace MusicGQL.Features.ServerLibrary.Cache;

public enum CachedReleaseDownloadStatus
{
    Idle,
    Searching,
    Downloading,
    NotFound,
}

public static class CachedReleaseDownloadStatusExtensions
{
    public static ReleaseDownloadStatus ToGql(this CachedReleaseDownloadStatus status) =>
        status switch
        {
            CachedReleaseDownloadStatus.Idle => ReleaseDownloadStatus.Idle,
            CachedReleaseDownloadStatus.Searching => ReleaseDownloadStatus.Searching,
            CachedReleaseDownloadStatus.Downloading => ReleaseDownloadStatus.Downloading,
            CachedReleaseDownloadStatus.NotFound => ReleaseDownloadStatus.NotFound,
            _ => ReleaseDownloadStatus.Idle,
        };
}
