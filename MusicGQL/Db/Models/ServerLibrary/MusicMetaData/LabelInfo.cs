namespace MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

public class LabelInfo
{
    public int Id { get; set; } // Not part of MusicBrainz
    public string CatalogNumber { get; set; }
    public Label Label { get; set; }
}
