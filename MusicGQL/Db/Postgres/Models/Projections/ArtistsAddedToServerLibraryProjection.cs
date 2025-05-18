namespace MusicGQL.Db.Postgres.Models.Projections;

public record ArtistsAddedToServerLibraryProjection
{
    public int Id { get; set; } = 1; // Singleton

    public List<string> ArtistMbIds { get; set; } = [];

    public DateTime LastUpdatedAt { get; set; }
};
