using System.ComponentModel.DataAnnotations;

namespace MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

public class Area
{
    [Key]
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Type { get; set; }
    public string? Disambiguation { get; set; }
    public List<string>? IsoCodes { get; set; }
}
