namespace MusicGQL.Db.Postgres.Models.Events;

public class LikedSong : Event
{
    public string RecordingId { get; set; }
}
