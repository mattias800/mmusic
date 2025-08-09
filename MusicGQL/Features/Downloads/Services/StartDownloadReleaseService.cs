using MusicGQL.Features.Downloads.Mutations;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Writer;
using Path = System.IO.Path;

namespace MusicGQL.Features.Downloads.Services;

public class StartDownloadReleaseService(
    ServerLibraryCache cache,
    SoulSeekReleaseDownloader soulSeekReleaseDownloader,
    ServerLibraryJsonWriter writer,
    ILogger<StartDownloadReleaseService> logger
)
{
    public async Task<(bool Success, string? ErrorMessage)> StartAsync(
        string artistId,
        string releaseFolderName,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "[StartDownload] Begin for {ArtistId}/{ReleaseFolder}",
            artistId,
            releaseFolderName
        );

        var release = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);

        if (release == null)
        {
            var msg = $"Release not found in cache: {artistId}/{releaseFolderName}";
            logger.LogWarning("[StartDownload] {Message}", msg);
            return (false, "Release not found in cache");
        }

        var artistName = release.ArtistName;
        var releaseTitle = release.Title;
        var targetDir = release.ReleasePath; // full path on disk

        logger.LogInformation(
            "[StartDownload] Resolved targetDir={TargetDir}, artistName='{Artist}', releaseTitle='{Title}'",
            targetDir,
            artistName,
            releaseTitle
        );

        var ok = await soulSeekReleaseDownloader.DownloadReleaseAsync(
            artistId,
            releaseFolderName,
            artistName,
            releaseTitle,
            targetDir
        );
        if (!ok)
        {
            var msg = $"No suitable download found for {artistName} - {releaseTitle}";
            logger.LogWarning("[StartDownload] {Message}", msg);
            return (false, "No suitable download found");
        }

        var releaseJsonPath = Path.Combine(targetDir, "release.json");
        logger.LogInformation("[StartDownload] Updating JSON at {Path}", releaseJsonPath);

        if (File.Exists(releaseJsonPath))
        {
            try
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

                logger.LogInformation(
                    "[StartDownload] Found {Count} audio files in {Dir}",
                    audioFiles.Count,
                    targetDir
                );

                await writer.UpdateReleaseAsync(
                    artistId,
                    releaseFolderName,
                    rel =>
                    {
                        if (rel.Tracks is null)
                            return;
                        for (int i = 0; i < rel.Tracks.Count; i++)
                        {
                            if (i < audioFiles.Count)
                            {
                                rel.Tracks[i].AudioFilePath = "./" + audioFiles[i];
                            }
                        }
                    }
                );

                logger.LogInformation("[StartDownload] Updated release.json with audio file paths");

                // Reload just this release into cache so it reflects new JSON (preserves transient availability)
                logger.LogInformation(
                    "[StartDownload] Refreshing release in cache after JSON update..."
                );
                await cache.UpdateReleaseFromJsonAsync(artistId, releaseFolderName);

                // Now publish availability status updates to reflect current runtime state
                var relAfterCount = audioFiles.Count; // used for bounds below
                await Task.WhenAll(
                    Enumerable
                        .Range(0, relAfterCount)
                        .Select(i =>
                            cache.UpdateMediaAvailabilityStatus(
                                artistId,
                                releaseFolderName,
                                i + 1,
                                CachedMediaAvailabilityStatus.Available
                            )
                        )
                );
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "[StartDownload] Failed updating release.json for {ArtistId}/{Folder}",
                    artistId,
                    releaseFolderName
                );
                return (false, "Failed to update release.json after download");
            }
        }

        logger.LogInformation("[StartDownload] Done");
        return (true, null);
    }
}
