namespace MusicGQL.ServerLibrary.Json;

public class ReleaseJson
{
    public string Title { get; set; }
    public ReleaseType Type { get; set; }
    public string? FirstReleaseDate { get; set; }
    public string? FirstReleaseYear { get; set; }
    public string? CoverArt { get; set; }
}

public enum ReleaseType
{
    Album,
    Ep,
    Single,
}

public class ReleaseServiceConnections
{
    public string? MusicBrainzReleaseGroupId { get; set; }
    public string? SpotifyAlbumId { get; set; }
    public string? YoutubePlaylistUrl { get; set; }
}
