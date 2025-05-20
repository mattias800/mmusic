namespace MusicGQL.Db.Postgres.Models.Events.Playlists;

public class SongAddedToPlaylist : Event
{
    public Guid PlaylistId { get; set; }
    public string RecordingId { get; set; } = string.Empty; // This is the Spotify Track ID
    public string TrackName { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty; // Primary artist name
    public int Position { get; set; }
}
