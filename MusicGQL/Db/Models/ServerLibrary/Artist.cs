namespace MusicGQL.Db.Models.ServerLibrary;

public class Artist
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public string SortName { get; set; }
    public Area Area { get; set; }
    public Area BeginArea { get; set; }
    public Area EndArea { get; set; }
    public string Country { get; set; }
    public string Disambiguation { get; set; }
}
