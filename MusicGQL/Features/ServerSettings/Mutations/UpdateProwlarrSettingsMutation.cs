using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.ServerSettings.Db;
using MusicGQL.Types;

namespace MusicGQL.Features.ServerSettings.Mutations;

public record UpdateProwlarrSettingsInput(
    string? BaseUrl,
    int TimeoutSeconds,
    int MaxRetries,
    int RetryDelaySeconds,
    bool TestConnectivityFirst,
    bool EnableDetailedLogging,
    int MaxConcurrentRequests
);

[UnionType]
public abstract record UpdateProwlarrSettingsResult;
public record UpdateProwlarrSettingsSuccess(ServerSettings ServerSettings) : UpdateProwlarrSettingsResult;
public record UpdateProwlarrSettingsError(string Message) : UpdateProwlarrSettingsResult;

[ExtendObjectType(typeof(Mutation))]
public class UpdateProwlarrSettingsMutation
{
    public async Task<UpdateProwlarrSettingsResult> UpdateProwlarrSettings(
        UpdateProwlarrSettingsInput input,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] EventDbContext dbContext,
        [Service] EventProcessor.EventProcessorWorker eventProcessorWorker
    )
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return new UpdateProwlarrSettingsError("Not authenticated");

        var viewer = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (viewer is null || (viewer.Roles & Users.Roles.UserRoles.Admin) == 0)
            return new UpdateProwlarrSettingsError("Not authorized");

        dbContext.Events.Add(new Events.ProwlarrSettingsUpdated
        {
            ActorUserId = userId,
            BaseUrl = input.BaseUrl,
            TimeoutSeconds = input.TimeoutSeconds,
            MaxRetries = input.MaxRetries,
            RetryDelaySeconds = input.RetryDelaySeconds,
            TestConnectivityFirst = input.TestConnectivityFirst,
            EnableDetailedLogging = input.EnableDetailedLogging,
            MaxConcurrentRequests = input.MaxConcurrentRequests,
        });
        await dbContext.SaveChangesAsync();
        await eventProcessorWorker.ProcessEvents();

        var settings = await dbContext.ServerSettings.FirstOrDefaultAsync(s => s.Id == DefaultDbServerSettingsProvider.ServerSettingsSingletonId)
            ?? DefaultDbServerSettingsProvider.GetDefault();
        return new UpdateProwlarrSettingsSuccess(new(settings));
    }
}


