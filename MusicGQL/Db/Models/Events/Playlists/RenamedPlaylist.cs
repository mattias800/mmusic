namespace MusicGQL.Db.Models.Events.Playlists;

public class RenamedPlaylist: Event
{
    public Guid PlaylistId { get; set; }
    public string NewPlaylistName { get; set; }
}