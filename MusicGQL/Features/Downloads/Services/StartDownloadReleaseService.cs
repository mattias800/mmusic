using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Writer;
using Path = System.IO.Path;

namespace MusicGQL.Features.Downloads.Services;

public class StartDownloadReleaseService(
    ServerLibraryCache cache,
    SoulSeekReleaseDownloader soulSeekReleaseDownloader,
    ServerLibraryJsonWriter writer,
    ILogger<StartDownloadReleaseService> logger,
    Features.Import.Services.MusicBrainzImportService mbImport,
    Features.Import.Services.LibraryReleaseImportService releaseImporter,
    HotChocolate.Subscriptions.ITopicEventSender eventSender
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

        // Set status to Searching before starting
        await cache.UpdateReleaseDownloadStatus(
            artistId,
            releaseFolderName,
            CachedReleaseDownloadStatus.Searching
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

            await cache.UpdateReleaseDownloadStatus(
                artistId,
                releaseFolderName,
                CachedReleaseDownloadStatus.NotFound
            );

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

        await cache.UpdateReleaseDownloadStatus(
            artistId,
            releaseFolderName,
            CachedReleaseDownloadStatus.Idle
        );

        // Auto-refresh metadata now that audio exists
        try
        {
            var rel = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);
            var artist = await cache.GetArtistByIdAsync(artistId);
            var mbArtistId = artist?.JsonArtist.Connections?.MusicBrainzArtistId;
            if (!string.IsNullOrWhiteSpace(mbArtistId) && rel != null)
            {
                var rgs = await mbImport.GetArtistReleaseGroupsAsync(mbArtistId!);
                var match = rgs.FirstOrDefault(rg =>
                    string.Equals(rg.Title, rel.Title, StringComparison.OrdinalIgnoreCase)
                );
                if (match != null)
                {
                    var importResult = await releaseImporter.ImportReleaseGroupAsync(
                        match,
                        Path.GetDirectoryName(rel.ReleasePath)
                            ?? Path.Combine("./Library", artistId),
                        artistId
                    );
                    if (importResult.Success)
                    {
                        await cache.UpdateReleaseFromJsonAsync(artistId, releaseFolderName);
                        // Publish metadata updated event
                        var updated = await cache.GetReleaseByArtistAndFolderAsync(
                            artistId,
                            releaseFolderName
                        );
                        if (updated != null)
                        {
                            await eventSender.SendAsync(
                                ServerLibrary.Subscription.LibrarySubscription.LibraryReleaseMetadataUpdatedTopic(
                                    artistId,
                                    releaseFolderName
                                ),
                                new ServerLibrary.Release(updated)
                            );
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[StartDownload] Auto-refresh after download failed");
        }

        // Finished
        logger.LogInformation("[StartDownload] Done");
        return (true, null);
    }
}
