using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.Playlists.Events;

public class RenamedPlaylist : Event
{
    public required string PlaylistId { get; set; }
    public required string NewPlaylistName { get; set; }
}
