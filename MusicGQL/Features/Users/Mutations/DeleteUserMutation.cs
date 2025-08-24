using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.EventProcessor;
using MusicGQL.Features.Users.Events;
using MusicGQL.Features.Users.Roles;
using MusicGQL.Types;

namespace MusicGQL.Features.Users.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class DeleteUserMutation
{
    public async Task<DeleteUserResult> DeleteUser(
        DeleteUserInput input,
        [Service] EventDbContext dbContext,
        [Service] EventProcessorWorker eventProcessor,
        ClaimsPrincipal claims
    )
    {
        var actorIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
        if (actorIdClaim is null || !Guid.TryParse(actorIdClaim.Value, out var actorUserId))
        {
            return new DeleteUserError("Not authenticated");
        }

        var actor = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == actorUserId);
        if (actor is null || (actor.Roles & (UserRoles.Admin | UserRoles.ManageUserRoles)) == 0)
        {
            return new DeleteUserError("Not authorized");
        }

        if (input.UserId == actorUserId)
        {
            return new DeleteUserError("You cannot delete yourself");
        }

        var target = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == input.UserId);
        if (target is null)
            return new DeleteUserError("User not found");

        var isAdmin = (target.Roles & UserRoles.Admin) != 0;
        if (isAdmin)
        {
            var adminCount = await dbContext.Users.CountAsync(u =>
                (u.Roles & UserRoles.Admin) != 0
            );
            if (adminCount <= 1)
            {
                return new DeleteUserError("Cannot delete the last admin user");
            }
        }

        dbContext.Events.Add(new UserDeleted { SubjectUserId = input.UserId });
        await dbContext.SaveChangesAsync();
        await eventProcessor.ProcessEvents();

        return new DeleteUserSuccess(input.UserId, new());
    }
}

public record DeleteUserInput(Guid UserId);

[UnionType]
public abstract record DeleteUserResult;

public record DeleteUserSuccess(Guid DeletedUserId, UserSearchRoot User) : DeleteUserResult;

public record DeleteUserError(string Message) : DeleteUserResult;
