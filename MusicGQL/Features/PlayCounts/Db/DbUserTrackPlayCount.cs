namespace MusicGQL.Features.PlayCounts.Db;

public class DbUserTrackPlayCount
{
    public int Id { get; set; }

    public required Guid UserId { get; set; }
    public required string ArtistId { get; set; }
    public required string ReleaseFolderName { get; set; }
    public required int TrackNumber { get; set; }

    public long PlayCount { get; set; }
    public DateTime? LastPlayedAt { get; set; }
}


