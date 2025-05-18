using System.ComponentModel.DataAnnotations;

namespace MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

public class Label
{
    [Key]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Disambiguation { get; set; }
    public List<Alias> Aliases { get; set; }
}
