namespace MusicGQL.Db.Models.Events;

public class LikedSong : Event
{
    public string RecordingId { get; set; }
}