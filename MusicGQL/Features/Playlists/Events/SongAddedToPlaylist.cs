using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.Playlists.Events;

public class SongAddedToPlaylist : Event
{
    public required Guid PlaylistId { get; set; }
    public required string RecordingId { get; set; }

    /**
     * If null, the song was added to the end of the playlist.
     */
    public int? Position { get; set; }
}
