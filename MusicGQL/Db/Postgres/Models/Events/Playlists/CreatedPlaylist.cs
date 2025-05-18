namespace MusicGQL.Db.Postgres.Models.Events.Playlists;

public class CreatedPlaylist : Event
{
    public Guid PlaylistId { get; set; }
}
