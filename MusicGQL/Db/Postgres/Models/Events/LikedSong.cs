using System; // For Guid

namespace MusicGQL.Db.Postgres.Models.Events;

public class LikedSong : Event
{
    public Guid SubjectUserId { get; set; }
    public string RecordingId { get; set; }
}
