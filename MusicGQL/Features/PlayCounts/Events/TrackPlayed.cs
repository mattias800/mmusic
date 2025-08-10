using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.PlayCounts.Events;

public class TrackPlayed : Event
{
    public required string ArtistId { get; set; }
    public required string ReleaseFolderName { get; set; }
    public required int TrackNumber { get; set; }

    // Who played the track
    public required Guid SubjectUserId { get; set; }

    // Denormalized for resilience/debugging
    public string? ArtistName { get; set; }
    public string? ReleaseTitle { get; set; }
    public string? TrackTitle { get; set; }
}


