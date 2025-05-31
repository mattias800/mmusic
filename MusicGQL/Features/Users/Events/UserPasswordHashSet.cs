using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.Users.Events;

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
