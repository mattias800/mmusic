using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.Users.Events;

public class UserUsernameUpdated : Event
{
    public Guid SubjectUserId { get; set; }
    public string NewUsername { get; set; } = string.Empty;
}
