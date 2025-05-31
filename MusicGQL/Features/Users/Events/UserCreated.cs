using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.Users.Events;

public class UserCreated : Event
{
    public Guid SubjectUserId { get; set; }
    public string Username { get; set; } = string.Empty;
    // No password here, that will be in a separate event or projection
}
