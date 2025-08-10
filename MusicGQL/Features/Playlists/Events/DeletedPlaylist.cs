using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.Playlists.Events;

public class DeletedPlaylist : Event
{
    public required string PlaylistId { get; set; }
}
