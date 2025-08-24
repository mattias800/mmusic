using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.EventProcessor;
using MusicGQL.Features.Users.Events;
using MusicGQL.Features.Users.Roles;
using MusicGQL.Types;

namespace MusicGQL.Features.Users.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class UpdateUserRolesMutation
{
    public async Task<UpdateUserRolesResult> UpdateUserRoles(
        UpdateUserRolesInput input,
        [Service] EventDbContext dbContext,
        [Service] EventProcessorWorker eventProcessor,
        ClaimsPrincipal claims
    )
    {
        var actorIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
        if (actorIdClaim is null || !Guid.TryParse(actorIdClaim.Value, out var actorUserId))
        {
            return new UpdateUserRolesError("Not authenticated");
        }

        var actor = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == actorUserId);
        if (actor is null)
        {
            return new UpdateUserRolesError("Actor not found");
        }

        var actorCanManage = (actor.Roles & (UserRoles.Admin | UserRoles.ManageUserRoles)) != 0;
        if (!actorCanManage)
        {
            return new UpdateUserRolesError("Not authorized to change user roles");
        }

        var targetUser = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == input.UserId);
        if (targetUser is null)
        {
            return new UpdateUserRolesError("User not found");
        }

        var newRoles = (UserRoles)input.Roles;

        var removingAdmin =
            (targetUser.Roles & UserRoles.Admin) != 0 && (newRoles & UserRoles.Admin) == 0;
        if (removingAdmin)
        {
            if (actorUserId == targetUser.UserId)
            {
                return new UpdateUserRolesError("You cannot remove admin from yourself");
            }

            var adminCount = await dbContext.Users.CountAsync(u =>
                (u.Roles & UserRoles.Admin) != 0
            );
            if (adminCount <= 1)
            {
                return new UpdateUserRolesError("Cannot remove admin from the last admin user");
            }
        }

        dbContext.Events.Add(
            new UserRolesUpdated { SubjectUserId = input.UserId, Roles = newRoles }
        );

        await dbContext.SaveChangesAsync();
        await eventProcessor.ProcessEvents();

        var updated = await dbContext.Users.FirstAsync(u => u.UserId == input.UserId);
        return new UpdateUserRolesSuccess(new User(updated));
    }
}

public record UpdateUserRolesInput(Guid UserId, int Roles);

[UnionType]
public abstract record UpdateUserRolesResult;

public record UpdateUserRolesSuccess(User User) : UpdateUserRolesResult;

public record UpdateUserRolesError(string Message) : UpdateUserRolesResult;
