using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Downloads.Services;
using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Types;

namespace MusicGQL.Features.Downloads.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class StartDownloadReleaseMutation
{
    [Authorize]
    public async Task<StartDownloadReleaseResult> StartDownloadRelease(
        ClaimsPrincipal claimsPrincipal,
        [Service] EventDbContext dbContext,
        [Service] ServerLibraryCache cache,
        [Service] DownloadQueueService queue,
        [Service] DownloadLogPathProvider logPathProvider,
        StartDownloadReleaseInput input
    )
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null)
        {
            return new StartDownloadReleaseUnknownError("Not authenticated");
        }

        var userId = Guid.Parse(userIdClaim.Value);
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user is null)
        {
            return new StartDownloadReleaseUnknownError("User not found");
        }

        var canTrigger =
            (user.Roles & (Users.Roles.UserRoles.TriggerDownloads | Users.Roles.UserRoles.Admin))
            != 0;
        if (!canTrigger)
        {
            return new StartDownloadReleaseUnknownError("Not authorized to trigger downloads");
        }

        // Enqueue to the FRONT for user-triggered priority (best effort)
        try
        {
            queue.EnqueueFront(new DownloadQueueItem(input.ArtistId, input.ReleaseFolderName));
        }
        catch { }

        // Get the release info for the response
        var release = await cache.GetReleaseByArtistAndFolderAsync(
            input.ArtistId,
            input.ReleaseFolderName
        );

        // Log to per-release log that the user initiated a download request and it was queued (priority)
        try
        {
            if (release is not null)
            {
                var path = await logPathProvider.GetReleaseLogFilePathAsync(
                    release.ArtistName,
                    release.Title
                );
                if (!string.IsNullOrWhiteSpace(path))
                {
                    using var relLogger = new DownloadLogger(path!);
                    relLogger.Info(
                        "[Queue] Download requested by user; enqueued at FRONT (priority)"
                    );
                }
            }
        }
        catch { }

        return release is null
            ? new StartDownloadReleaseAccepted(input.ArtistId, input.ReleaseFolderName)
            : new StartDownloadReleaseSuccess(new Release(release));
    }
}

public record StartDownloadReleaseInput(string ArtistId, string ReleaseFolderName);

[UnionType("StartDownloadReleaseResult")]
public abstract record StartDownloadReleaseResult { };

public record StartDownloadReleaseSuccess(Release Release) : StartDownloadReleaseResult;

public record StartDownloadReleaseAccepted(string ArtistId, string ReleaseFolderName)
    : StartDownloadReleaseResult;

public record StartDownloadReleaseUnknownError(string Message) : StartDownloadReleaseResult;
