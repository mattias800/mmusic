using System.ComponentModel.DataAnnotations;

namespace MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

public class Release
{
    [Key]
    public string Id { get; set; }
    public string Title { get; set; }
    public string Status { get; set; }
    public string Quality { get; set; }
    public TextRepresentation TextRepresentation { get; set; }
    public string Date { get; set; }
    public string Country { get; set; }
    public string? Barcode { get; set; }
    public ReleaseGroup ReleaseGroup { get; set; }
    public CoverArtArchive CoverArtArchive { get; set; }
    public List<NameCredit> Credits { get; set; }
    public List<LabelInfo> Labels { get; set; }
    public List<Medium> Media { get; set; }
    public List<Tag> Tags { get; set; }
    public List<Genre> Genres { get; set; }
    public List<Relation> Relations { get; set; }
    public List<Alias> Aliases { get; set; }
}
