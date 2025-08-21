using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.EventProcessor;
using MusicGQL.Features.Users.Events;
using MusicGQL.Features.Users.Roles;
using MusicGQL.Types;

namespace MusicGQL.Features.Users.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class UpdateUserUsernameMutation
{
    public async Task<UpdateUserUsernameResult> UpdateUserUsername(
        UpdateUserUsernameInput input,
        [Service] EventDbContext dbContext,
        [Service] EventProcessorWorker eventProcessor,
        ClaimsPrincipal claims
    )
    {
        var actorIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
        if (actorIdClaim is null || !Guid.TryParse(actorIdClaim.Value, out var actorUserId))
        {
            return new UpdateUserUsernameError("Not authenticated");
        }

        var actor = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == actorUserId);
        if (actor is null || (actor.Roles & (UserRoles.Admin | UserRoles.ManageUserRoles)) == 0)
        {
            return new UpdateUserUsernameError("Not authorized");
        }

        var target = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == input.UserId);
        if (target is null) return new UpdateUserUsernameError("User not found");

        // Ensure unique username
        var exists = await dbContext.Users.AnyAsync(u => u.Username == input.NewUsername && u.UserId != input.UserId);
        if (exists) return new UpdateUserUsernameError("Username already taken");

        dbContext.Events.Add(new UserUsernameUpdated
        {
            SubjectUserId = input.UserId,
            NewUsername = input.NewUsername,
        });

        await dbContext.SaveChangesAsync();
        await eventProcessor.ProcessEvents();

        var updated = await dbContext.Users.FirstAsync(u => u.UserId == input.UserId);
        return new UpdateUserUsernameSuccess(new User(updated));
    }
}

public record UpdateUserUsernameInput(Guid UserId, string NewUsername);

[UnionType]
public abstract record UpdateUserUsernameResult;

public record UpdateUserUsernameSuccess(User User) : UpdateUserUsernameResult;

public record UpdateUserUsernameError(string Message) : UpdateUserUsernameResult;


