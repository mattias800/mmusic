using System.Text.Json;
using System.Text.Json.Serialization;
using MusicGQL.Features.Downloads.Services;
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
        [Service] ILogger<StartDownloadReleaseMutation> logger,
        StartDownloadReleaseInput input
    )
    {
        logger.LogInformation("[StartDownload] Begin for {ArtistId}/{ReleaseFolder}", input.ArtistId, input.ReleaseFolderName);
        var release = await cache.GetReleaseByArtistAndFolderAsync(
            input.ArtistId,
            input.ReleaseFolderName
        );

        if (release == null)
        {
            logger.LogWarning("[StartDownload] Release not found in cache: {ArtistId}/{ReleaseFolder}", input.ArtistId, input.ReleaseFolderName);
            return new StartDownloadReleaseUnknownError("Release not found in cache");
        }

        var artistName = release.ArtistName;
        var releaseTitle = release.Title;
        var targetDir = release.ReleasePath; // full path on disk

        logger.LogInformation("[StartDownload] Resolved targetDir={TargetDir}, artistName='{Artist}', releaseTitle='{Title}'",
            targetDir, artistName, releaseTitle);

        var ok = await soulSeekReleaseDownloader.DownloadReleaseAsync(
            input.ArtistId,
            input.ReleaseFolderName,
            artistName,
            releaseTitle,
            targetDir
        );
        if (!ok)
        {
            logger.LogWarning("[StartDownload] No suitable download found for {Artist} - {Title}", artistName, releaseTitle);
            return new StartDownloadReleaseUnknownError("No suitable download found");
        }

        var releaseJsonPath = Path.Combine(targetDir, "release.json");
        logger.LogInformation("[StartDownload] Updating JSON at {Path}", releaseJsonPath);
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

                    logger.LogInformation("[StartDownload] Found {Count} audio files in {Dir}", audioFiles.Count, targetDir);
                    for (int i = 0; i < json.Tracks.Count; i++)
                    {
                        if (i < audioFiles.Count)
                        {
                            json.Tracks[i].AudioFilePath = "./" + audioFiles[i];
                        }
                    }

                    var updated = JsonSerializer.Serialize(json, GetJsonOptions());
                    await File.WriteAllTextAsync(releaseJsonPath, updated);

                    logger.LogInformation("[StartDownload] Updated release.json with audio file paths");
                    for (int i = 0; i < json.Tracks.Count; i++)
                    {
                        if (i < audioFiles.Count)
                        {
                            await cache.UpdateMediaAvailabilityStatus(
                                input.ArtistId,
                                input.ReleaseFolderName,
                                i + 1,
                                CachedMediaAvailabilityStatus.Available
                            );
                            logger.LogInformation("[StartDownload] Marked track {Track} as Available", i + 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[StartDownload] Failed updating release.json for {ArtistId}/{Folder}", input.ArtistId, input.ReleaseFolderName);
            }
        }

        logger.LogInformation("[StartDownload] Refreshing library cache...");
        await cache.UpdateCacheAsync();
        logger.LogInformation("[StartDownload] Done");
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

public record StartDownloadReleaseInput(string ArtistId, string ReleaseFolderName);

[UnionType("StartDownloadReleaseResult")]
public abstract record StartDownloadReleaseResult { };

public record StartDownloadReleaseSuccess(bool Success) : StartDownloadReleaseResult;

public record StartDownloadReleaseUnknownError(string Message) : StartDownloadReleaseResult;
