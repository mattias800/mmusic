namespace MusicGQL.Db.Postgres.Models.Events.Playlists;

public class RenamedPlaylist : Event
{
    public required Guid PlaylistId { get; set; }
    public required string NewPlaylistName { get; set; }
}
