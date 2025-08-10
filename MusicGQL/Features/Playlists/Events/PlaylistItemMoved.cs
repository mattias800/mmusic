using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.Playlists.Events;

public class PlaylistItemMoved : Event
{
    public required Guid PlaylistId { get; set; }
    public required string RecordingId { get; set; }
    public required int NewIndex { get; set; }
}


