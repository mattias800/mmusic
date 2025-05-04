namespace MusicGQL.Db.Models.Events.Playlists;

public class CreatedPlaylist : Event
{
    public Guid PlaylistId { get; set; }
}
