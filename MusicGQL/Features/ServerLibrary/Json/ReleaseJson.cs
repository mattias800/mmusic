namespace MusicGQL.Features.ServerLibrary.Json;

public class ReleaseJson
{
    public string Title { get; set; }
    public string? SortTitle { get; set; }
    public ReleaseType Type { get; set; }
    public List<TrackJson>? Tracks { get; set; }
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

public class TrackJson
{
    public string Title { get; set; }
    public string? SortTitle { get; set; }
    public int TrackNumber { get; set; }
    public int TrackLength { get; set; }
    public string? AudioFilePath { get; set; }
    public TrackServiceConnections? Connections { get; set; }
}

public class TrackServiceConnections
{
    public string? MusicBrainzRecordingId { get; set; }
    public string? SpotifySongId { get; set; }
    public string? MusicVideoYoutubeVideoUrl { get; set; }
}
