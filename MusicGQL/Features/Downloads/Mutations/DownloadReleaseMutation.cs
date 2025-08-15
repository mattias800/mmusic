using MusicGQL.Features.Downloads.Services;
using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Types;

namespace MusicGQL.Features.Downloads.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class StartDownloadReleaseMutation
{
    public async Task<StartDownloadReleaseResult> StartDownloadRelease(
        [Service] ServerLibraryCache cache,
        [Service] DownloadQueueService queue,
        StartDownloadReleaseInput input
    )
    {
        // Enqueue to the FRONT for user-triggered priority (best effort)
        try { queue.EnqueueFront(new DownloadQueueItem(input.ArtistId, input.ReleaseFolderName)); } catch { }

        // Get the release info for the response
        var release = await cache.GetReleaseByArtistAndFolderAsync(input.ArtistId, input.ReleaseFolderName);
        return release is null
            ? new StartDownloadReleaseAccepted(input.ArtistId, input.ReleaseFolderName)
            : new StartDownloadReleaseSuccess(new Release(release));
    }
}

public record StartDownloadReleaseInput(string ArtistId, string ReleaseFolderName);

[UnionType("StartDownloadReleaseResult")]
public abstract record StartDownloadReleaseResult { };

public record StartDownloadReleaseSuccess(Release Release) : StartDownloadReleaseResult;

public record StartDownloadReleaseAccepted(string ArtistId, string ReleaseFolderName) : StartDownloadReleaseResult;

public record StartDownloadReleaseUnknownError(string Message) : StartDownloadReleaseResult;
