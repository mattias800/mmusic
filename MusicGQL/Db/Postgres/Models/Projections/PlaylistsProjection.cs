using System.ComponentModel.DataAnnotations;

namespace MusicGQL.Db.Postgres.Models.Projections;

public record PlaylistsProjection
{
    [Key]
    public Guid UserId { get; set; }

    public List<PlaylistProjection> Playlists { get; set; } = new();
}

public record PlaylistProjection
{
    [Key]
    public Guid Id { get; set; }

    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }

    public List<string> RecordingIds { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
