namespace MusicGQL.Features.Downloads;

public enum MediaAvailabilityStatus
{
    Unknown,
    Missing,
    QueuedForDownload,
    Downloading,
    Processing,
    Available,
}
