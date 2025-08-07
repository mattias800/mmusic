namespace MusicGQL.Features.ServerLibrary.Json;

public class JsonArtist
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? SortName { get; set; }
    public long? MonthlyListeners { get; set; }
    public List<JsonTopTrack>? TopTracks { get; set; }
    public JsonArtistPhotos? Photos { get; set; }
    public JsonArtistServiceConnections? Connections { get; set; }
}

public class JsonTopTrack
{
    public string Title { get; set; } = string.Empty;
    public string? ReleaseTitle { get; set; }
    public string? CoverArtUrl { get; set; }
    public string? ReleaseFolderName { get; set; }
    public int? TrackNumber { get; set; }
    public long? PlayCount { get; set; }
}

public class JsonArtistPhotos
{
    public List<string>? Backgrounds { get; set; }
    public List<string>? Thumbs { get; set; }
    public List<string>? Banners { get; set; }
    public List<string>? Logos { get; set; }
}

public class JsonArtistServiceConnections
{
    public string? MusicBrainzArtistId { get; set; }
    public string? SpotifyId { get; set; }
    public string? YoutubeChannelUrl { get; set; }
}
