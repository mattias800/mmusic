namespace MusicGQL.Features.PlayCounts.Db;

public class DbTrackPlayCount
{
    public int Id { get; set; }

    public required string ArtistId { get; set; }
    public required string ReleaseFolderName { get; set; }
    public required int TrackNumber { get; set; }

    // Denormalized fields for resilience/debugging
    public string? ArtistName { get; set; }
    public string? ReleaseTitle { get; set; }
    public string? TrackTitle { get; set; }

    // Total play counts across all users
    public long PlayCount { get; set; }
    public DateTime? LastPlayedAt { get; set; }
}


