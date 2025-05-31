using System.ComponentModel.DataAnnotations;

namespace MusicGQL.Features.Likes.Db;

public record LikedSongsProjection
{
    [Key]
    public Guid UserId { get; set; }

    public List<string> LikedSongRecordingIds { get; set; } = new(); // Ensure initialized

    public DateTime LastUpdatedAt { get; set; }
}
