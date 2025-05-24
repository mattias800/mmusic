namespace MusicGQL.Db.Postgres.Models.Events;

public class UnlikedSong : Event
{
    public Guid SubjectUserId { get; set; }
    public string RecordingId { get; set; }
}
