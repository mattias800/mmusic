using MusicGQL.Features.ServerLibrary.Cache;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class RedownloadReleaseMutation
{
    public async Task<RedownloadReleaseResult> RedownloadRelease(
        [Service] ServerLibraryCache cache,
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
        var releaseJsonPath = Path.Combine(dir, "release.json");
        try
        {
            if (File.Exists(releaseJsonPath))
            {
                var jsonText = await File.ReadAllTextAsync(releaseJsonPath);
                var json = System.Text.Json.JsonSerializer.Deserialize<MusicGQL.Features.ServerLibrary.Json.JsonRelease>(jsonText, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase) }
                });
                if (json?.Tracks != null)
                {
                    foreach (var t in json.Tracks)
                    {
                        t.AudioFilePath = null;
                    }
                    var updated = System.Text.Json.JsonSerializer.Serialize(json, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                        WriteIndented = true,
                        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase) }
                    });
                    await File.WriteAllTextAsync(releaseJsonPath, updated);
                }
            }
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

