using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MusicGQL.Db.Postgres.Models.Projections;

public record LikedSongsProjection
{
    [Key]
    public Guid UserId { get; set; }

    public List<string> LikedSongRecordingIds { get; set; } = new(); // Ensure initialized

    public DateTime LastUpdatedAt { get; set; }
}
