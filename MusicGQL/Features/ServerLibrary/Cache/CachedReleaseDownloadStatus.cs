using MusicGQL.Features.Downloads;

namespace MusicGQL.Features.ServerLibrary.Cache;

/// <summary>
/// Represents the current operational state of a download
/// </summary>
public enum DownloadState
{
    Idle,           // No download activity
    Searching,      // Searching for download sources
    Downloading,    // Files are being downloaded
    ImportingFiles, // Downloaded files are being imported/processed
    MatchingRelease, // Matching downloaded content to MusicBrainz releases
    ValidatingTracks, // Validating track count matches
    Finished        // Download process completed (regardless of result)
}

/// <summary>
/// Represents the final outcome of a download attempt
/// </summary>
public enum DownloadResult
{
    NoResultYet,        // Download is still in progress
    NoSearchResult,     // No download sources found
    NoMatchingReleases, // Files downloaded but no matching MusicBrainz release
    TrackCountMismatch, // Files exist but track count doesn't match expected
    MetadataRefreshFailed, // Metadata refresh process failed
    Cancelled,          // User-initiated cancellation
    UnknownError,       // Unexpected error occurred
    Success             // Download completed successfully with valid files
}

/// <summary>
/// Legacy enum for backward compatibility - maps to DownloadState
/// </summary>
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
