namespace MusicGQL.Db.Neo4j.ServerLibrary.MusicMetaData;

public class Relation
{
    public string? Type { get; set; }
    public string? TargetType { get; set; }
    public string? TypeId { get; set; }
    public string? Direction { get; set; }
    public string? Begin { get; set; }
    public string? End { get; set; }
    public List<string>? Attributes { get; set; }
    public string? Url { get; set; }
}
