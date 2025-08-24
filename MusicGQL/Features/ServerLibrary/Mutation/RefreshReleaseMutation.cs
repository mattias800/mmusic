using System.Text.Json;
using System.Text.Json.Serialization;
using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ServerLibrary.Writer;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class RefreshReleaseMutation
{
    public async Task<RefreshReleaseResult> RefreshRelease(
        ServerLibraryCache cache,
        ArtistImportQueue.Services.ArtistImportQueueService artistQueue,
        RefreshReleaseInput input
    )
    {
        var release = await cache.GetReleaseByArtistAndFolderAsync(
            input.ArtistId,
            input.ReleaseFolderName
        );
        if (release == null)
        {
            return new RefreshReleaseError("Release not found");
        }

        // Enqueue in existing ArtistImport queue as a release refresh job for unification
        var artist = await cache.GetArtistByIdAsync(input.ArtistId);
        var queueItem = new ArtistImportQueue.ArtistImportQueueItem(
            artist?.Name ?? input.ArtistId,
            null
        )
        {
            JobKind = ArtistImportQueue.ArtistImportJobKind.RefreshReleaseMetadata,
            LocalArtistId = input.ArtistId,
            ReleaseFolderName = input.ReleaseFolderName,
        };
        artistQueue.Enqueue(queueItem);
        return new RefreshReleaseSuccess(new(release));
    }
}

public record RefreshReleaseInput(string ArtistId, string ReleaseFolderName);

[UnionType("RefreshReleaseResult")]
public abstract record RefreshReleaseResult;

public record RefreshReleaseSuccess(Release Release) : RefreshReleaseResult;

public record RefreshReleaseError(string Message) : RefreshReleaseResult;
