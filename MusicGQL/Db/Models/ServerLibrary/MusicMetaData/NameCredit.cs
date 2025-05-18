namespace MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

public class NameCredit
{
    public int Id { get; set; } // Not part of MusicBrainz
    public string JoinPhrase { get; set; }
    public string Name { get; set; }
    public Artist Artist { get; set; }
}
