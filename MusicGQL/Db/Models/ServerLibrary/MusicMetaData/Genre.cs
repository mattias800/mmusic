using System.ComponentModel.DataAnnotations;

namespace MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

public class Genre
{
    [Key]
    public string Id { get; set; }
    public string Name { get; set; }
    public int Count { get; set; }
    public string Disambiguation { get; set; }
}
