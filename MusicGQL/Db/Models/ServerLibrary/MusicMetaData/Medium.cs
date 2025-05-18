namespace MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

public class Medium
{
    public int Id { get; set; } // Not part of MusicBrainz
    public int TrackCount { get; set; }
    public int Position { get; set; }
    public string Format { get; set; }
    public List<Disc> Discs { get; set; }
    public List<Track> Tracks { get; set; }
}
