namespace MusicGQL.Db.Models.Projections;

public record ReleaseGroupsAddedToServerLibraryProjection
{
    public int Id { get; set; } = 1; // Singleton

    public List<string> ReleaseGroupMbIds { get; set; } = [];

    public DateTime LastUpdatedAt { get; set; }
};
