using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.Users.Events;

public class UserDeleted : Event
{
    public Guid SubjectUserId { get; set; }
}


