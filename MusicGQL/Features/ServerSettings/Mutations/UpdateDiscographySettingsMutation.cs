using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.ServerSettings.Db;
using MusicGQL.Types;

namespace MusicGQL.Features.ServerSettings.Mutations;

public record UpdateDiscographySettingsInput(bool Enabled, string? StagingPath);

[UnionType]
public abstract record UpdateDiscographySettingsResult;

public record UpdateDiscographySettingsSuccess(ServerSettings ServerSettings)
    : UpdateDiscographySettingsResult;

public record UpdateDiscographySettingsError(string Message) : UpdateDiscographySettingsResult;

[ExtendObjectType(typeof(Mutation))]
public class UpdateDiscographySettingsMutation
{
    public async Task<UpdateDiscographySettingsResult> UpdateDiscographySettings(
        UpdateDiscographySettingsInput input,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] EventDbContext dbContext,
        [Service] EventProcessor.EventProcessorWorker worker
    )
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );
        if (!Guid.TryParse(userIdString, out var userId))
            return new UpdateDiscographySettingsError("Not authenticated");

        var viewer = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (viewer is null || (viewer.Roles & Users.Roles.UserRoles.Admin) == 0)
            return new UpdateDiscographySettingsError("Not authorized");

        dbContext.Events.Add(
            new Events.DiscographySettingsUpdated
            {
                ActorUserId = userId,
                Enabled = input.Enabled,
                StagingPath = input.StagingPath,
            }
        );
        await dbContext.SaveChangesAsync();
        await worker.ProcessEvents();

        var settings =
            await dbContext.ServerSettings.FirstOrDefaultAsync(s =>
                s.Id == DefaultDbServerSettingsProvider.ServerSettingsSingletonId
            ) ?? DefaultDbServerSettingsProvider.GetDefault();
        return new UpdateDiscographySettingsSuccess(new(settings));
    }
}
