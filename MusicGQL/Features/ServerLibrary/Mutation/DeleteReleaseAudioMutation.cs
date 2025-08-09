using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Writer;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class DeleteReleaseAudioMutation
{
    public async Task<DeleteReleaseAudioResult> DeleteReleaseAudio(
        [Service] ServerLibraryCache cache,
        [Service] ServerLibraryJsonWriter writer,
        DeleteReleaseAudioInput input
    )
    {
        var release = await cache.GetReleaseByArtistAndFolderAsync(
            input.ArtistId,
            input.ReleaseFolderName
        );
        if (release is null)
        {
            return new DeleteReleaseAudioError("Release not found");
        }

        // Delete all audio files referenced by tracks (best-effort)
        var errors = new List<string>();
        if (release.JsonRelease.Tracks != null)
        {
            foreach (var t in release.JsonRelease.Tracks)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(t.AudioFilePath))
                    {
                        var rel = t.AudioFilePath!.StartsWith("./")
                            ? t.AudioFilePath![2..]
                            : t.AudioFilePath!;
                        var fullPath = Path.Combine(release.ReleasePath, rel);
                        // Ensure the path resides under the release directory
                        var fullPathNormalized = Path.GetFullPath(fullPath);
                        var releasePathNormalized = Path.GetFullPath(release.ReleasePath);
                        if (
                            fullPathNormalized.StartsWith(
                                releasePathNormalized,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                        {
                            if (File.Exists(fullPathNormalized))
                            {
                                File.Delete(fullPathNormalized);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }
            }
        }

        // Clear AudioFilePath from release.json
        try
        {
            await writer.UpdateReleaseAsync(
                input.ArtistId,
                input.ReleaseFolderName,
                r =>
                {
                    if (r.Tracks == null)
                        return;
                    foreach (var jt in r.Tracks)
                    {
                        jt.AudioFilePath = null;
                    }
                }
            );
        }
        catch (Exception ex)
        {
            return new DeleteReleaseAudioError($"Failed to update release.json: {ex.Message}");
        }

        // Update cache for this release
        await cache.UpdateReleaseFromJsonAsync(input.ArtistId, input.ReleaseFolderName);

        // Mark each track as Missing in cache (and notify subscribers)
        if (release.JsonRelease.Tracks != null)
        {
            foreach (var t in release.JsonRelease.Tracks)
            {
                try
                {
                    await cache.UpdateMediaAvailabilityStatus(
                        input.ArtistId,
                        input.ReleaseFolderName,
                        t.TrackNumber,
                        CachedMediaAvailabilityStatus.Missing
                    );
                }
                catch { }
            }
        }

        var updated = await cache.GetReleaseByArtistAndFolderAsync(
            input.ArtistId,
            input.ReleaseFolderName
        );
        if (updated is null)
        {
            return new DeleteReleaseAudioError("Release not found after update");
        }

        return new DeleteReleaseAudioSuccess(new Release(updated));
    }
}

[UnionType("DeleteReleaseAudioResult")]
public abstract record DeleteReleaseAudioResult;

public record DeleteReleaseAudioSuccess(Release Release) : DeleteReleaseAudioResult;

public record DeleteReleaseAudioError(string Message) : DeleteReleaseAudioResult;

public record DeleteReleaseAudioInput(string ArtistId, string ReleaseFolderName);
