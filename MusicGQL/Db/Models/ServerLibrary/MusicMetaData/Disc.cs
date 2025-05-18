using System.ComponentModel.DataAnnotations;

namespace MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

public class Disc
{
    [Key]
    public string Id { get; set; }
    public int Sectors { get; set; }
    public List<int> Offsets { get; set; }
}
