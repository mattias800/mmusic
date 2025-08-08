using MusicGQL.Features.Downloads;

namespace MusicGQL.Features.ServerLibrary.Cache;

public enum CachedMediaAvailabilityStatus
{
    Unknown,
    Missing,
    QueuedForDownload,
    Downloading,
    Processing,
    Available,
}

public static class CachedMediaAvailabilityStatusExtensions
{
    public static MediaAvailabilityStatus ToGql(this CachedMediaAvailabilityStatus status) =>
        status switch
        {
            CachedMediaAvailabilityStatus.Unknown => MediaAvailabilityStatus.Unknown,
            CachedMediaAvailabilityStatus.Missing => MediaAvailabilityStatus.Missing,
            CachedMediaAvailabilityStatus.QueuedForDownload =>
                MediaAvailabilityStatus.QueuedForDownload,
            CachedMediaAvailabilityStatus.Downloading => MediaAvailabilityStatus.Downloading,
            CachedMediaAvailabilityStatus.Available => MediaAvailabilityStatus.Available,
            _ => MediaAvailabilityStatus.Unknown,
        };

    public static bool IsAvailable(this CachedMediaAvailabilityStatus status) =>
        status == CachedMediaAvailabilityStatus.Available;

    public static bool IsMissing(this CachedMediaAvailabilityStatus status) =>
        status == CachedMediaAvailabilityStatus.Missing;

    public static bool IsDownloading(this CachedMediaAvailabilityStatus status) =>
        status == CachedMediaAvailabilityStatus.Downloading;

    public static bool IsQueuedForDownload(this CachedMediaAvailabilityStatus status) =>
        status == CachedMediaAvailabilityStatus.QueuedForDownload;
}
