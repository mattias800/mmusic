namespace MusicGQL.Db.Models.Events.Playlists;

public class AddedSongToPlaylist: Event
{
    public Guid PlaylistId { get; set; }
    public string RecordingId { get; set; }
}