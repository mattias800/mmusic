namespace MusicGQL.Features.ServerLibrary2.Cache;

public class CacheStatistics
{
    public int ArtistCount { get; set; }
    public int ReleaseCount { get; set; }
    public int TrackCount { get; set; }
    public DateTime LastUpdated { get; set; }
    public bool IsInitialized { get; set; }
} 