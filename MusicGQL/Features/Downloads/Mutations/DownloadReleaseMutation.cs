using System.Text.Json;
using System.Text.Json.Serialization;
using MusicGQL.Features.Downloads.Services;
using MusicGQL.Features.External.SoulSeek.Integration;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Types;
using Path = System.IO.Path;

namespace MusicGQL.Features.Downloads.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class StartDownloadReleaseMutation
{
    public async Task<StartDownloadReleaseResult> StartDownloadRelease(
        [Service] ServerLibraryCache cache,
        [Service] SoulSeekReleaseDownloader soulSeekReleaseDownloader,
        StartDownloadReleaseInput input
    )
    {
        var parts = input.ReleaseId.Split('/');
        if (parts.Length != 2)
        {
            return new StartDownloadReleaseUnknownError("Invalid release id");
        }

        var artistId = parts[0];
        var releaseFolder = parts[1];

        var release = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolder);
        if (release == null)
        {
            return new StartDownloadReleaseUnknownError("Release not found in cache");
        }

        var artistName = release.ArtistName;
        var releaseTitle = release.Title;
        var targetDir = release.ReleasePath;

        var ok = await soulSeekReleaseDownloader.DownloadReleaseAsync(
            artistName,
            releaseTitle,
            targetDir
        );
        if (!ok)
        {
            return new StartDownloadReleaseUnknownError("No suitable download found");
        }

        var releaseJsonPath = Path.Combine(targetDir, "release.json");
        if (File.Exists(releaseJsonPath))
        {
            try
            {
                var jsonText = await File.ReadAllTextAsync(releaseJsonPath);
                var json = JsonSerializer.Deserialize<JsonRelease>(jsonText, GetJsonOptions());
                if (json?.Tracks != null && json.Tracks.Count > 0)
                {
                    var audioFiles = Directory
                        .GetFiles(targetDir)
                        .Where(f =>
                            new[] { ".mp3", ".flac", ".wav", ".m4a", ".ogg" }.Contains(
                                Path.GetExtension(f).ToLowerInvariant()
                            )
                        )
                        .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                        .Select(Path.GetFileName)
                        .ToList();

                    for (int i = 0; i < json.Tracks.Count; i++)
                    {
                        if (i < audioFiles.Count)
                        {
                            json.Tracks[i].AudioFilePath = "./" + audioFiles[i];
                        }
                    }

                    var updated = JsonSerializer.Serialize(json, GetJsonOptions());
                    await File.WriteAllTextAsync(releaseJsonPath, updated);
                }
            }
            catch { }
        }

        await cache.UpdateCacheAsync();
        return new StartDownloadReleaseSuccess(true);
    }

    private static JsonSerializerOptions GetJsonOptions() =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };
}

public record StartDownloadReleaseInput(string ReleaseId);

[UnionType("StartDownloadReleaseResult")]
public abstract record StartDownloadReleaseResult { };

public record StartDownloadReleaseSuccess(bool Success) : StartDownloadReleaseResult;

public record StartDownloadReleaseUnknownError(string Message) : StartDownloadReleaseResult;
