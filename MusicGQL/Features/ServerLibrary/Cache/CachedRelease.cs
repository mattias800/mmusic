using MusicGQL.Features.ServerLibrary.Json;

namespace MusicGQL.Features.ServerLibrary.Cache;

public class CachedRelease
{
    public string Title { get; set; } = string.Empty;
    public string? SortTitle { get; set; }
    public Json.JsonReleaseType Type { get; set; }
    public string ReleasePath { get; set; } = string.Empty;
    public string FolderName { get; set; } = string.Empty;
    public string ArtistId { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public JsonRelease JsonRelease { get; set; } = new();
    public List<CachedTrack> Tracks { get; set; } = new();

    // Searchable properties
    public string SearchFolderName => FolderName.ToLowerInvariant();
    public string SearchTitle => Title.ToLowerInvariant();
    public string? SearchSortTitle => SortTitle?.ToLowerInvariant();
    public string SearchArtistName => ArtistName.ToLowerInvariant();
}
