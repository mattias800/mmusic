namespace MusicGQL.Db.Postgres.Models.Events.Users;

public class UserPasswordHashSet : Event
{
    public Guid SubjectUserId { get; init; }
    public string PasswordHash { get; init; } = string.Empty;

    public UserPasswordHashSet(Guid subjectUserId, string passwordHash)
    {
        SubjectUserId = subjectUserId;
        PasswordHash = passwordHash;
    }
}
