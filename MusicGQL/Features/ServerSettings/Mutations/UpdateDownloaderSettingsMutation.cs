using HotChocolate;
using MusicGQL.Db.Postgres;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MusicGQL.Types;

namespace MusicGQL.Features.ServerSettings.Mutations;

public record UpdateDownloaderSettingsInput(
    bool EnableSabnzbdDownloader,
    bool EnableQBittorrentDownloader,
    bool EnableSoulSeekDownloader
);

[UnionType]
public abstract record UpdateDownloaderSettingsResult;
public record UpdateDownloaderSettingsSuccess(ServerSettings ServerSettings) : UpdateDownloaderSettingsResult;
public record UpdateDownloaderSettingsError(string Message) : UpdateDownloaderSettingsResult;

[ExtendObjectType(typeof(Mutation))]
public class UpdateDownloaderSettingsMutation
{
    public async Task<UpdateDownloaderSettingsResult> UpdateDownloaderSettings(
        UpdateDownloaderSettingsInput input,
        [Service] EventDbContext dbContext,
        ClaimsPrincipal claims)
    {
        try
        {
            var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim is null) return new UpdateDownloaderSettingsError("Not authenticated");
            var userId = Guid.Parse(userIdClaim.Value);
            var viewer = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (viewer is null || (viewer.Roles & Users.Roles.UserRoles.Admin) == 0)
            {
                return new UpdateDownloaderSettingsError("Not authorized");
            }

            var settings = await dbContext.ServerSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                return new UpdateDownloaderSettingsError("Server settings not found");
            }

            settings.EnableSabnzbdDownloader = input.EnableSabnzbdDownloader;
            settings.EnableQBittorrentDownloader = input.EnableQBittorrentDownloader;
            settings.EnableSoulSeekDownloader = input.EnableSoulSeekDownloader;

            await dbContext.SaveChangesAsync();

            return new UpdateDownloaderSettingsSuccess(new(settings));
        }
        catch (Exception ex)
        {
            return new UpdateDownloaderSettingsError($"Failed to update downloader settings: {ex.Message}");
        }
    }
}


