namespace MusicGQL.Db.Neo4j.ServerLibrary.MusicMetaData;

public class Recording
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int? Length { get; set; }
    public string? Disambiguation { get; set; }
    // Add other relevant properties from Hqub.MusicBrainz.Entities.Recording as needed
    // Relationships like ArtistCredits, Releases will be handled via Neo4j relationships
    // public List<Relation>? Relations { get; set; } // Decide how to model relations
}
