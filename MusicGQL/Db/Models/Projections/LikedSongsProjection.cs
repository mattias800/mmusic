namespace MusicGQL.Db.Models.Projections;

public record LikedSongsProjection
{
    // TODO User specific
    public int Id { get; set; } = 1; // Singleton
    
    public List<string> LikedSongReleaseIds { get; set; } = [];
    
    public DateTime LastUpdatedAt { get; set; }
}