namespace MusicGQL.Features.ServerLibrary.Common.Db;

public class DbRelation
{
    public string Type { get; set; }
    public string TypeId { get; set; }
    public string TargetType { get; set; }
    public string Direction { get; set; }
    public string Begin { get; set; }
    public string End { get; set; }
    public bool? Ended { get; set; }
    public string[] Attributes { get; set; }
    public string Url { get; set; }
    // public Artist Artist { get; set; }
    // public Work Work { get; set; }
}
