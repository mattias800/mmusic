namespace MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

public class Alias
{
    public string Name { get; set; }
    public string SortName { get; set; }
    public string Type { get; set; }
    public string Locale { get; set; }
    public string Begin { get; set; }
    public string End { get; set; }
    public bool? Ended { get; set; }
    public bool? Primary { get; set; }
}
