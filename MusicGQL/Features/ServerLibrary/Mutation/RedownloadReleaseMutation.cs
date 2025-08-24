using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Writer;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class RedownloadReleaseMutation
{
    public async Task<RedownloadReleaseResult> RedownloadRelease(
        RedownloadReleaseInput input,
        [Service] ServerLibraryCache cache,
        [Service] ServerLibraryJsonWriter writer
    )
    {
        var release = await cache.GetReleaseByArtistAndFolderAsync(
            input.ArtistId,
            input.ReleaseFolderName
        );
        if (release == null)
        {
            return new RedownloadReleaseError("Release not found");
        }

        var dir = release.ReleasePath;
        try
        {
            if (Directory.Exists(dir))
            {
                var audioExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    ".mp3",
                    ".flac",
                    ".wav",
                    ".m4a",
                    ".ogg",
                };
                foreach (var file in Directory.GetFiles(dir))
                {
                    var ext = Path.GetExtension(file);
                    if (audioExts.Contains(ext))
                    {
                        File.Delete(file);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return new RedownloadReleaseError($"Failed deleting audio files: {ex.Message}");
        }

        // Update release.json to clear audio references
        try
        {
            await writer.UpdateReleaseAsync(
                input.ArtistId,
                input.ReleaseFolderName,
                rel =>
                {
                    if (rel.Tracks is null)
                        return;
                    foreach (var t in rel.Tracks)
                    {
                        t.AudioFilePath = null;
                    }
                }
            );
        }
        catch (Exception ex)
        {
            return new RedownloadReleaseError($"Failed updating release.json: {ex.Message}");
        }

        await cache.UpdateCacheAsync();
        var updated = await cache.GetReleaseByArtistAndFolderAsync(
            input.ArtistId,
            input.ReleaseFolderName
        );
        if (updated is null)
        {
            return new RedownloadReleaseError("Release not found after redownload");
        }
        return new RedownloadReleaseSuccess(new Release(updated));
    }
}

[UnionType("RedownloadReleaseResult")]
public abstract record RedownloadReleaseResult;

public record RedownloadReleaseSuccess(Release Release) : RedownloadReleaseResult;

public record RedownloadReleaseError(string Message) : RedownloadReleaseResult;

public record RedownloadReleaseInput(string ArtistId, string ReleaseFolderName);
