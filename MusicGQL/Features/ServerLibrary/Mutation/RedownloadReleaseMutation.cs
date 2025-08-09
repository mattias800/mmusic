using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Writer;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class RedownloadReleaseMutation
{
    public async Task<RedownloadReleaseResult> RedownloadRelease(
        [Service] ServerLibraryCache cache,
        [Service] ServerLibraryJsonWriter writer,
        string ArtistId,
        string ReleaseFolderName
    )
    {
        var release = await cache.GetReleaseByArtistAndFolderAsync(ArtistId, ReleaseFolderName);
        if (release == null)
        {
            return new RedownloadReleaseError("Release not found");
        }

        var dir = release.ReleasePath;
        try
        {
            if (Directory.Exists(dir))
            {
                var audioExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".mp3", ".flac", ".wav", ".m4a", ".ogg" };
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
                ArtistId,
                ReleaseFolderName,
                rel =>
                {
                    if (rel.Tracks is null) return;
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
        return new RedownloadReleaseSuccess(true);
    }
}

[UnionType("RedownloadReleaseResult")]
public abstract record RedownloadReleaseResult;

public record RedownloadReleaseSuccess(bool Success) : RedownloadReleaseResult;

public record RedownloadReleaseError(string Message) : RedownloadReleaseResult;

