using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.Likes.Events;

public class UnlikedSong : Event
{
    public Guid SubjectUserId { get; set; }
    public string RecordingId { get; set; }
}
