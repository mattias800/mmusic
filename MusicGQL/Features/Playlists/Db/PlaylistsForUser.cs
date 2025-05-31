using System.ComponentModel.DataAnnotations;

namespace MusicGQL.Features.Playlists.Db;

public class PlaylistsForUser
{
    [Key]
    public required Guid UserId { get; set; }

    public List<DbPlaylist> Playlists { get; set; } = new();
}
