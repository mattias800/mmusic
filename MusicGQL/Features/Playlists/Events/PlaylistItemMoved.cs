using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.Playlists.Events;

public class PlaylistItemMoved : Event
{
    public required string PlaylistId { get; set; }
    public required string PlaylistItemId { get; set; }
    public required int NewIndex { get; set; }
}
