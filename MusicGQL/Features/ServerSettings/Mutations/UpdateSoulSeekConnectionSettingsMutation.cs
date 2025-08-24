using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.ServerSettings.Db;
using MusicGQL.Types;

namespace MusicGQL.Features.ServerSettings.Mutations;

public record UpdateSoulSeekConnectionSettingsInput(string Host, int Port, string Username);

[UnionType]
public abstract record UpdateSoulSeekConnectionSettingsResult;

public record UpdateSoulSeekConnectionSettingsSuccess(ServerSettings ServerSettings)
    : UpdateSoulSeekConnectionSettingsResult;

public record UpdateSoulSeekConnectionSettingsError(string Message)
    : UpdateSoulSeekConnectionSettingsResult;

[ExtendObjectType(typeof(Mutation))]
public class UpdateSoulSeekConnectionSettingsMutation
{
    public async Task<UpdateSoulSeekConnectionSettingsResult> UpdateSoulSeekConnectionSettings(
        UpdateSoulSeekConnectionSettingsInput input,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] EventDbContext dbContext,
        [Service] EventProcessor.EventProcessorWorker eventProcessorWorker
    )
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return new UpdateSoulSeekConnectionSettingsError("Not authenticated");
        }

        var viewer = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (viewer is null || (viewer.Roles & Users.Roles.UserRoles.Admin) == 0)
        {
            return new UpdateSoulSeekConnectionSettingsError("Not authorized");
        }

        dbContext.Events.Add(
            new Events.SoulSeekConnectionUpdated
            {
                ActorUserId = userId,
                Host = input.Host,
                Port = input.Port,
                Username = input.Username,
            }
        );
        await dbContext.SaveChangesAsync();
        await eventProcessorWorker.ProcessEvents();

        var settings =
            await dbContext.ServerSettings.FirstOrDefaultAsync(s =>
                s.Id == DefaultDbServerSettingsProvider.ServerSettingsSingletonId
            ) ?? DefaultDbServerSettingsProvider.GetDefault();
        return new UpdateSoulSeekConnectionSettingsSuccess(new(settings));
    }
}
