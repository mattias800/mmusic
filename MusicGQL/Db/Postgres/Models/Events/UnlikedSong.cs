namespace MusicGQL.Db.Postgres.Models.Events;

public class UnlikedSong : Event
{
    public string RecordingId { get; set; }
}
