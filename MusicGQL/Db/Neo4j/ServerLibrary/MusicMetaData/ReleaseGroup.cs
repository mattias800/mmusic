using System.Collections.Generic;

namespace MusicGQL.Db.Neo4j.ServerLibrary.MusicMetaData;

public class ReleaseGroup
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PrimaryType { get; set; } = string.Empty;
    public List<string> SecondaryTypes { get; set; } = new List<string>();
    public string FirstReleaseDate { get; set; } = string.Empty;
}
