using MusicGQL.Features.Users.Db;

namespace MusicGQL.Features.Playlists.Db;

public class DbPlaylist
{
    public string Id { get; set; }

    public Guid UserId { get; set; }
    public DbUser User { get; set; } = null!; // optional, but useful for navigation

    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    public List<DbPlaylistItem> Items { get; set; } = new();
}
