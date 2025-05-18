namespace MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

public class Tag
{
    public int Id { get; set; } // Not part of MusicBrainz
    public int Count { get; set; }
    public string Name { get; set; }
}
