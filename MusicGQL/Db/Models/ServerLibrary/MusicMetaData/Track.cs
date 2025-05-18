using System.ComponentModel.DataAnnotations;

namespace MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

public class Track
{
    public string Id { get; set; } // From MusicBrainz API
    public string Number { get; set; }
    public int Position { get; set; }
    public int? Length { get; set; }
    public Recording Recording { get; set; }
}
