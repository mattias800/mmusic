using System.Security.Claims;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Downloads.Services;
using MusicGQL.Types;

namespace MusicGQL.Features.ServerSettings.Mutations;

public record UpdateDownloaderSettingsInput(
    bool EnableSabnzbdDownloader,
    bool EnableQBittorrentDownloader,
    bool EnableSoulSeekDownloader
);

[UnionType]
public abstract record UpdateDownloaderSettingsResult;

public record UpdateDownloaderSettingsSuccess(ServerSettings ServerSettings)
    : UpdateDownloaderSettingsResult;

public record UpdateDownloaderSettingsError(string Message) : UpdateDownloaderSettingsResult;

[ExtendObjectType(typeof(Mutation))]
public class UpdateDownloaderSettingsMutation
{
    public async Task<UpdateDownloaderSettingsResult> UpdateDownloaderSettings(
        UpdateDownloaderSettingsInput input,
        [Service] EventDbContext dbContext,
        ClaimsPrincipal claims,
        [Service] DownloadLogPathProvider logPathProvider
    )
    {
        try
        {
            var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim is null)
                return new UpdateDownloaderSettingsError("Not authenticated");
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

            bool prevSab = settings.EnableSabnzbdDownloader;
            bool prevQb = settings.EnableQBittorrentDownloader;
            bool prevSlsk = settings.EnableSoulSeekDownloader;

            settings.EnableSabnzbdDownloader = input.EnableSabnzbdDownloader;
            settings.EnableQBittorrentDownloader = input.EnableQBittorrentDownloader;
            settings.EnableSoulSeekDownloader = input.EnableSoulSeekDownloader;

            await dbContext.SaveChangesAsync();

            // Log changes to per-service logs
            try
            {
                if (prevSab != settings.EnableSabnzbdDownloader)
                {
                    var path = await logPathProvider.GetServiceLogFilePathAsync("sabnzbd");
                    if (!string.IsNullOrWhiteSpace(path))
                        using (var l = new DownloadLogger(path!))
                            l.Info(
                                $"Toggle changed: enabled={settings.EnableSabnzbdDownloader} by user={userId}"
                            );
                }
            }
            catch { }
            try
            {
                if (prevQb != settings.EnableQBittorrentDownloader)
                {
                    var path = await logPathProvider.GetServiceLogFilePathAsync("qbittorrent");
                    if (!string.IsNullOrWhiteSpace(path))
                        using (var l = new DownloadLogger(path!))
                            l.Info(
                                $"Toggle changed: enabled={settings.EnableQBittorrentDownloader} by user={userId}"
                            );
                }
            }
            catch { }
            try
            {
                if (prevSlsk != settings.EnableSoulSeekDownloader)
                {
                    var path = await logPathProvider.GetServiceLogFilePathAsync("soulseek");
                    if (!string.IsNullOrWhiteSpace(path))
                        using (var l = new DownloadLogger(path!))
                            l.Info(
                                $"Toggle changed: enabled={settings.EnableSoulSeekDownloader} by user={userId}"
                            );
                }
            }
            catch { }

            return new UpdateDownloaderSettingsSuccess(new(settings));
        }
        catch (Exception ex)
        {
            return new UpdateDownloaderSettingsError(
                $"Failed to update downloader settings: {ex.Message}"
            );
        }
    }
}
