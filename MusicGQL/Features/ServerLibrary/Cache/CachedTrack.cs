using MusicGQL.Features.ServerLibrary.Json;

namespace MusicGQL.Features.ServerLibrary.Cache;

public class CachedTrack
{
    public string Title { get; set; } = string.Empty;
    public string? SortTitle { get; set; }
    public int TrackNumber { get; set; }
    public int DiscNumber { get; set; } = 1;
    public string? AudioFilePath { get; set; }

    public string ArtistId { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public string ReleaseFolderName { get; set; } = string.Empty;
    public string ReleaseTitle { get; set; } = string.Empty;
    public JsonReleaseType JsonReleaseType { get; set; }
    public JsonTrack JsonTrack { get; set; } = new();

    // Searchable properties
    public string SearchTitle => Title.ToLowerInvariant();
    public string? SearchSortTitle => SortTitle?.ToLowerInvariant();
    public string SearchArtistName => ArtistName.ToLowerInvariant();
    public string SearchReleaseTitle => ReleaseTitle.ToLowerInvariant();
    public string SearchReleaseFolderName => ReleaseFolderName.ToLowerInvariant();

    // Download status
    public CachedMediaAvailabilityStatus CachedMediaAvailabilityStatus { get; set; } =
        CachedMediaAvailabilityStatus.Unknown;
}
