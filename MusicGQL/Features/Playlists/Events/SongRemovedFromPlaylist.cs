using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.Playlists.Events;

public class SongRemovedFromPlaylist : Event
{
    public required string PlaylistId { get; set; }
    public required string PlaylistItemId { get; set; }
}
