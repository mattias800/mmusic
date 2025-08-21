using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.ServerSettings.Db;
using MusicGQL.Types;

namespace MusicGQL.Features.ServerSettings.Mutations;

public record UpdateQBittorrentSettingsInput(
    string? BaseUrl,
    string? Username,
    string? SavePath
);

[UnionType]
public abstract record UpdateQBittorrentSettingsResult;
public record UpdateQBittorrentSettingsSuccess(ServerSettings ServerSettings) : UpdateQBittorrentSettingsResult;
public record UpdateQBittorrentSettingsError(string Message) : UpdateQBittorrentSettingsResult;

[ExtendObjectType(typeof(Mutation))]
public class UpdateQBittorrentSettingsMutation
{
    public async Task<UpdateQBittorrentSettingsResult> UpdateQBittorrentSettings(
        UpdateQBittorrentSettingsInput input,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] EventDbContext dbContext,
        [Service] EventProcessor.EventProcessorWorker eventProcessorWorker
    )
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return new UpdateQBittorrentSettingsError("Not authenticated");

        var viewer = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (viewer is null || (viewer.Roles & Users.Roles.UserRoles.Admin) == 0)
            return new UpdateQBittorrentSettingsError("Not authorized");

        dbContext.Events.Add(new Events.QBittorrentSettingsUpdated
        {
            ActorUserId = userId,
            BaseUrl = input.BaseUrl,
            Username = input.Username,
            SavePath = input.SavePath,
        });
        await dbContext.SaveChangesAsync();
        await eventProcessorWorker.ProcessEvents();

        var settings = await dbContext.ServerSettings.FirstOrDefaultAsync(s => s.Id == DefaultDbServerSettingsProvider.ServerSettingsSingletonId)
            ?? DefaultDbServerSettingsProvider.GetDefault();
        return new UpdateQBittorrentSettingsSuccess(new(settings));
    }
}


