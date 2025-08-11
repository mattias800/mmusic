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

        // Delete ALL audio files under the release folder (regardless of references in release.json)
        var errors = new List<string>();
        try
        {
            var audioExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".mp3", ".flac", ".wav", ".m4a", ".ogg" };

            var releaseRoot = Path.GetFullPath(release.ReleasePath);
            var files = Directory
                .EnumerateFiles(releaseRoot, "*.*", SearchOption.AllDirectories)
                .Where(f => audioExts.Contains(Path.GetExtension(f)));

            foreach (var file in files)
            {
                try
                {
                    var normalized = Path.GetFullPath(file);
                    if (normalized.StartsWith(releaseRoot, StringComparison.OrdinalIgnoreCase)
                        && File.Exists(normalized))
                    {
                        File.Delete(normalized);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add(ex.Message);
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
