using MusicGQL.Features.ServerLibrary2.Json;

namespace MusicGQL.Features.ServerLibrary2.Cache;

public class CachedArtist
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? SortName { get; set; }
    public string ArtistPath { get; set; } = string.Empty;
    public ArtistJson ArtistJson { get; set; } = new();
    public List<CachedRelease> Releases { get; set; } = new();
    
    // Searchable properties
    public string SearchName => Name.ToLowerInvariant();
    public string? SearchSortName => SortName?.ToLowerInvariant();
    public string SearchId => Id.ToLowerInvariant();
} 