using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.Playlists.Events;

public class SongRemovedFromPlaylist : Event
{
    public required Guid PlaylistId { get; set; }
    public required string RecordingId { get; set; }
}


