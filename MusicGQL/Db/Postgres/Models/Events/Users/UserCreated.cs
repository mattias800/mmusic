namespace MusicGQL.Db.Postgres.Models.Events.Users;

public class UserCreated : Event
{
    public Guid SubjectUserId { get; set; }
    public string Username { get; set; } = string.Empty;
    // No password here, that will be in a separate event or projection
}
