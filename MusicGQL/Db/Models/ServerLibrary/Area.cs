namespace MusicGQL.Db.Models.ServerLibrary;

public class Area
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Type { get; set; }
    public string? Disambiguation { get; set; }
    public List<string>? IsoCodes { get; set; }
}
