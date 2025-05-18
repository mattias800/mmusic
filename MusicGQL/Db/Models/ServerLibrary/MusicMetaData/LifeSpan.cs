namespace MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

public class LifeSpan
{
    public int Id { get; set; } // Not part of MusicBrainz
    public string? Begin { get; set; }
    public string? End { get; set; }
    public bool? Ended { get; set; }
}
