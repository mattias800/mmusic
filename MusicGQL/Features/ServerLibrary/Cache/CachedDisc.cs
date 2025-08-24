namespace MusicGQL.Features.ServerLibrary.Cache;

public class CachedDisc
{
    public int DiscNumber { get; set; }
    public string? Title { get; set; }
    public List<CachedTrack> Tracks { get; set; } = new();
}
