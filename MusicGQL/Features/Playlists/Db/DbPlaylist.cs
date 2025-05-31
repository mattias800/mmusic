using System.ComponentModel.DataAnnotations;

namespace MusicGQL.Features.Playlists.Db;

public class DbPlaylist
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; } // FK to PlaylistsForUser
    public PlaylistsForUser User { get; set; } = null!;

    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public List<DbPlaylistItem> Items { get; set; } = new();
}
