using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.Playlists.Events;

public class ConnectPlaylistToExternalPlaylist : Event
{
    public required string PlaylistId { get; set; }
    public required ExternalServiceType ExternalService { get; set; }
    public required string ExternalPlaylistId { get; set; }
}
