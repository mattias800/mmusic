namespace MusicGQL.Db.Neo4j.ServerLibrary.MusicMetaData;

public class Artist
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SortName { get; set; } = string.Empty;
    public string? Gender { get; set; }
}
