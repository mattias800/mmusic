using System.ComponentModel.DataAnnotations;

namespace MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

public class Url
{
    [Key]
    public string Id { get; set; }
    public string Resource { get; set; }
}
