namespace MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

public class Rating
{
    public int Id { get; set; } // Not part of MusicBrainz
    public int VotesCount { get; set; }
    public double? Value { get; set; }
}
