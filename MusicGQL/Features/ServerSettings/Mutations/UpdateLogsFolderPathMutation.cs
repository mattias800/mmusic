using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.ServerSettings.Commands;
using MusicGQL.Features.ServerSettings.Db;
using MusicGQL.Types;

namespace MusicGQL.Features.ServerSettings.Mutations;

public record UpdateLogsFolderPathInput(string? NewPath);

[UnionType]
public abstract record UpdateLogsFolderPathResult;
public record UpdateLogsFolderPathSuccess(ServerSettings ServerSettings) : UpdateLogsFolderPathResult;
public record UpdateLogsFolderPathError(string Message) : UpdateLogsFolderPathResult;

[ExtendObjectType(typeof(Mutation))]
public class UpdateLogsFolderPathMutation
{
    public async Task<UpdateLogsFolderPathResult> UpdateLogsFolderPath(
        UpdateLogsFolderPathInput input,
        [Service] UpdateLogsFolderPathHandler handler,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] EventDbContext dbContext
    )
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return new UpdateLogsFolderPathError("Not authenticated");

        var viewer = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (viewer is null || (viewer.Roles & Users.Roles.UserRoles.Admin) == 0)
            return new UpdateLogsFolderPathError("Not authorized");

        var result = await handler.Handle(new UpdateLogsFolderPathHandler.Command(userId, input.NewPath));

        var settings = await dbContext.ServerSettings.FirstOrDefaultAsync(s => s.Id == DefaultDbServerSettingsProvider.ServerSettingsSingletonId)
            ?? DefaultDbServerSettingsProvider.GetDefault();
        return new UpdateLogsFolderPathSuccess(new(settings));
    }
}


