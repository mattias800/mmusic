using MusicGQL.Db.Postgres.Models;
using MusicGQL.Features.Users.Roles;

namespace MusicGQL.Features.Users.Events;

public class UserRolesUpdated : Event
{
    public Guid SubjectUserId { get; set; }
    public UserRoles Roles { get; set; }
}


