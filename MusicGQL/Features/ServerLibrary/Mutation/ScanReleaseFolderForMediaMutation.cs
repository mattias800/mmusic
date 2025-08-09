using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class ScanReleaseFolderForMediaMutation
{
    public async Task<ScanReleaseFolderForMediaResult> ScanReleaseFolderForMedia(
        [Service] ServerLibraryCache cache,
        [Service] MediaFileAssignmentService assigner,
        ScanReleaseFolderForMediaInput input
    )
    {
        var release = await cache.GetReleaseByArtistAndFolderAsync(
            input.ArtistId,
            input.ReleaseFolderName
        );
        if (release == null)
        {
            return new ScanReleaseFolderForMediaError("Release not found");
        }

        var ok = await assigner.AssignAsync(input.ArtistId, input.ReleaseFolderName);
        if (!ok)
        {
            return new ScanReleaseFolderForMediaError("No audio files found or failed to assign");
        }

        await cache.UpdateReleaseFromJsonAsync(input.ArtistId, input.ReleaseFolderName);
        var updated = await cache.GetReleaseByArtistAndFolderAsync(
            input.ArtistId,
            input.ReleaseFolderName
        );
        if (updated == null)
        {
            return new ScanReleaseFolderForMediaError("Failed to reload release after update");
        }

        return new ScanReleaseFolderForMediaSuccess(new(updated));
    }
}

public record ScanReleaseFolderForMediaInput(string ArtistId, string ReleaseFolderName);

[UnionType("ScanReleaseFolderForMediaResult")]
public abstract record ScanReleaseFolderForMediaResult;

public record ScanReleaseFolderForMediaSuccess(Release Release) : ScanReleaseFolderForMediaResult;

public record ScanReleaseFolderForMediaError(string Message) : ScanReleaseFolderForMediaResult;
