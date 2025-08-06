using MusicGQL.Features.ServerLibrary2.Json;

namespace MusicGQL.Features.ServerLibrary2.Cache;

public class CachedRelease
{
    public string Title { get; set; } = string.Empty;
    public string? SortTitle { get; set; }
    public ReleaseType Type { get; set; }
    public string ReleasePath { get; set; } = string.Empty;
    public string ArtistId { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public ReleaseJson ReleaseJson { get; set; } = new();
    public List<CachedTrack> Tracks { get; set; } = new();
    
    // Searchable properties
    public string SearchTitle => Title.ToLowerInvariant();
    public string? SearchSortTitle => SortTitle?.ToLowerInvariant();
    public string SearchArtistName => ArtistName.ToLowerInvariant();
} 