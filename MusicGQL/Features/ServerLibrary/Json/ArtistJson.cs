namespace MusicGQL.Features.ServerLibrary.Json;

public class ArtistJson
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? SortName { get; set; }
    public ArtistPhotosJson? Photos { get; set; }
    public ArtistServiceConnections? Connections { get; set; }
}

public class ArtistPhotosJson
{
    public List<string>? Backgrounds { get; set; }
    public List<string>? Thumbs { get; set; }
    public List<string>? Banners { get; set; }
    public List<string>? Logos { get; set; }
}

public class ArtistServiceConnections
{
    public string? MusicBrainzArtistId { get; set; }
    public string? SpotifyId { get; set; }
    public string? YoutubeChannelUrl { get; set; }
}
