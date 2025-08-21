using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.EventProcessor;
using MusicGQL.Features.Authentication.Commands;
using MusicGQL.Features.Authentication.Handlers;
using MusicGQL.Features.Users.Events;
using MusicGQL.Features.Users.Roles;
using MusicGQL.Types;

namespace MusicGQL.Features.Users.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class SetUserPasswordMutation
{
    public async Task<SetUserPasswordResult> SetUserPassword(
        SetUserPasswordInput input,
        [Service] EventDbContext dbContext,
        [Service] EventProcessorWorker eventProcessor,
        [Service] HashPasswordHandler hashPassword,
        ClaimsPrincipal claims
    )
    {
        var actorIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
        if (actorIdClaim is null || !Guid.TryParse(actorIdClaim.Value, out var actorUserId))
        {
            return new SetUserPasswordError("Not authenticated");
        }

        var actor = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == actorUserId);
        if (actor is null)
        {
            return new SetUserPasswordError("Actor not found");
        }

        var isSelf = actorUserId == input.UserId;
        var canSet = isSelf || (actor.Roles & (UserRoles.Admin | UserRoles.ManageUserRoles)) != 0;
        if (!canSet)
        {
            return new SetUserPasswordError("Not authorized");
        }

        var target = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == input.UserId);
        if (target is null) return new SetUserPasswordError("User not found");

        var hashed = await hashPassword.Handle(new HashPasswordCommand(input.NewPassword));

        dbContext.Events.Add(new UserPasswordHashUpdated(input.UserId, hashed.HashedPassword));

        await dbContext.SaveChangesAsync();
        await eventProcessor.ProcessEvents();

        var updated = await dbContext.Users.FirstAsync(u => u.UserId == input.UserId);
        return new SetUserPasswordSuccess(new User(updated));
    }
}

public record SetUserPasswordInput(Guid UserId, string NewPassword);

[UnionType]
public abstract record SetUserPasswordResult;

public record SetUserPasswordSuccess(User User) : SetUserPasswordResult;

public record SetUserPasswordError(string Message) : SetUserPasswordResult;


