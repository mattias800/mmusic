using System.ComponentModel.DataAnnotations;

namespace MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

public class Work
{
    [Key]
    public string Id { get; set; }
    public string Language { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public string ISWC { get; set; }
    public string Disambiguation { get; set; }
    public List<Relation> Relations { get; set; }
    public List<Alias> Aliases { get; set; }
}
