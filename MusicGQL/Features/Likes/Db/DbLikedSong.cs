using MusicGQL.Features.Users.Db;

namespace MusicGQL.Features.Likes.Db;

public class DbLikedSong
{
    public int Id { get; set; }

    public Guid LikedByUserId { get; set; }
    public DbUser LikedBy { get; set; } = null!;

    public required string RecordingId { get; set; }
    public DateTime LikedAt { get; set; }
}
