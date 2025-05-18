namespace MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

public class Disc
{
    public string Id { get; set; } // From MusicBrainz API
    public int Sectors { get; set; }
    public List<int> Offsets { get; set; }
}
