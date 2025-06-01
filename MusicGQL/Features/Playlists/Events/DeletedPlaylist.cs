using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.Playlists.Events;

public class DeletedPlaylist : Event
{
    public required Guid PlaylistId { get; set; }
}
