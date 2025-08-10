using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.Playlists.Events;

public class CreatedPlaylist : Event
{
    public required string PlaylistId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
}
