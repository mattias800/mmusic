namespace MusicGQL.ServerLibrary.Json;

public class TrackJson
{
    public string Title { get; set; }
    public int TrackNumber { get; set; }
    public string AudioFilePath { get; set; }
}

public class TrackServiceConnections
{
    public string? MusicBrainzRecordingId { get; set; }
    public string? SpotifySongId { get; set; }
    public string? MusicVideoYoutubeVideoUrl { get; set; }
}
