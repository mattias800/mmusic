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
        [Service] EventProcessorWorker eventProcessor
    )
    {
        var targetUser = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == input.UserId);
        if (targetUser is null)
        {
            return new UpdateUserRolesError("User not found");
        }

        dbContext.Events.Add(new UserRolesUpdated
        {
            SubjectUserId = input.UserId,
            Roles = (UserRoles)input.Roles,
        });

        await dbContext.SaveChangesAsync();
        await eventProcessor.ProcessEvents();

        var updated = await dbContext.Users.FirstAsync(u => u.UserId == input.UserId);
        return new UpdateUserRolesSuccess(new Users.User(updated));
    }
}

public record UpdateUserRolesInput(Guid UserId, int Roles);

[UnionType]
public abstract record UpdateUserRolesResult;

public record UpdateUserRolesSuccess(Users.User User) : UpdateUserRolesResult;

public record UpdateUserRolesError(string Message) : UpdateUserRolesResult;


