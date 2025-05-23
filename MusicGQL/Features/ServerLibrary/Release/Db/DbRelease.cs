namespace MusicGQL.Features.ServerLibrary.Release.Db;

public class DbRelease
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Date { get; set; }
    public string? Status { get; set; }
    // Add other relevant properties from Hqub.MusicBrainz.Entities.Release as needed
    // For example: Barcode, Country, Quality, Disambiguation
    // Relationships like ArtistCredits, Media, ReleaseGroup will be handled via Neo4j relationships
}
