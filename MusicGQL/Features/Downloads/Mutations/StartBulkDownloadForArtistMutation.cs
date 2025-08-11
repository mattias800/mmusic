using MusicGQL.Features.Downloads.Services;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Types;

namespace MusicGQL.Features.Downloads.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class StartBulkDownloadForArtistMutation
{
    public async Task<StartBulkDownloadForArtistResult> StartBulkDownloadForArtist(
        [Service] StartDownloadReleaseService service,
        [Service] ServerLibraryCache cache,
        [Service] Services.DownloadQueueService queue,
        StartBulkDownloadForArtistInput input
    )
    {
        var artist = await cache.GetArtistByIdAsync(input.ArtistId);

        if (artist == null)
        {
            return new StartBulkDownloadForArtistError("Artist not found");
        }

        var releases = artist.Releases;
        if (input.Scope == BulkDownloadScope.Albums)
            releases = releases.Where(r => r.Type == ServerLibrary.Json.JsonReleaseType.Album).ToList();
        else if (input.Scope == BulkDownloadScope.Eps)
            releases = releases.Where(r => r.Type == ServerLibrary.Json.JsonReleaseType.Ep).ToList();
        else if (input.Scope == BulkDownloadScope.Singles)
            releases = releases.Where(r => r.Type == ServerLibrary.Json.JsonReleaseType.Single).ToList();

        int queued = 0;
        foreach (var r in releases)
        {
            try { queue.Enqueue(new Features.Downloads.DownloadQueueItem(input.ArtistId, r.FolderName)); } catch { }
            var (ok, _) = await service.StartAsync(input.ArtistId, r.FolderName);
            if (ok) queued++;
        }

        return new StartBulkDownloadForArtistSuccess(input.ArtistId, queued);
    }
}

public record StartBulkDownloadForArtistInput(string ArtistId, BulkDownloadScope Scope);

public enum BulkDownloadScope
{
    All,
    Albums,
    Singles,
    Eps
}

[UnionType("StartBulkDownloadForArtistResult")]
public abstract record StartBulkDownloadForArtistResult;

public record StartBulkDownloadForArtistSuccess(string ArtistId, int QueuedCount) : StartBulkDownloadForArtistResult;

public record StartBulkDownloadForArtistError(string Message) : StartBulkDownloadForArtistResult;