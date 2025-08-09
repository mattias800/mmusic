namespace MusicGQL.Features.ServerLibrary.Json;

public class JsonRelease
{
    public string Title { get; set; }
    public string? SortTitle { get; set; }
    public JsonReleaseType Type { get; set; }
    public List<JsonTrack>? Tracks { get; set; }
    public string? FirstReleaseDate { get; set; }
    public string? FirstReleaseYear { get; set; }
    public string? CoverArt { get; set; }
}

public enum JsonReleaseType
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

public class JsonTrack
{
    public string Title { get; set; }
    public string? SortTitle { get; set; }
    public int TrackNumber { get; set; }
    public int? TrackLength { get; set; }
    public string? AudioFilePath { get; set; }
    public long? PlayCount { get; set; }
    public JsonTrackServiceConnections? Connections { get; set; }
    public List<JsonTrackCredit>? Credits { get; set; }
    public JsonTrackStatistics? Statistics { get; set; }
}

public class JsonTrackCredit
{
    public string ArtistName { get; set; }
    public string? ArtistId { get; set; }
    public string? MusicBrainzArtistId { get; set; }
}

public class JsonTrackServiceConnections
{
    public string? MusicBrainzRecordingId { get; set; }
    public string? SpotifySongId { get; set; }
    public string? MusicVideoYoutubeVideoUrl { get; set; }
}

public class JsonTrackStatistics
{
    public long Listeners { get; set; }
    public long PlayCount { get; set; }
}
