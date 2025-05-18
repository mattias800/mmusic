using System.ComponentModel.DataAnnotations;

namespace MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

public class Track
{
    [Key]
    public string Id { get; set; }
    public string Number { get; set; }
    public int Position { get; set; }
    public int? Length { get; set; }
    public Recording Recording { get; set; }
}
