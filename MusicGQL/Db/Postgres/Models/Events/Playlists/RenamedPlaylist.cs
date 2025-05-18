namespace MusicGQL.Db.Postgres.Models.Events.Playlists;

public class RenamedPlaylist : Event
{
    public Guid PlaylistId { get; set; }
    public string NewPlaylistName { get; set; }
}
