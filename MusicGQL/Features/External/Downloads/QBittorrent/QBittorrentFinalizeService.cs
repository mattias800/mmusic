using Microsoft.Extensions.Options;
using MusicGQL.Features.External.Downloads.QBittorrent;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerSettings;

namespace MusicGQL.Features.External.Downloads.QBittorrent;

public class QBittorrentFinalizeService(
    QBittorrentClient qb,
    ServerLibraryCache cache,
    ServerSettingsAccessor serverSettingsAccessor,
    ILogger<QBittorrentFinalizeService> logger,
    MusicGQL.Features.Downloads.Services.DownloadLogPathProvider logPathProvider
) : IQBittorrentFinalizeService
{
    private MusicGQL.Features.Downloads.Services.DownloadLogger? serviceLogger;

    public async Task<MusicGQL.Features.Downloads.Services.DownloadLogger> GetLogger()
    {
        if (serviceLogger == null)
        {
            var path = await logPathProvider.GetServiceLogFilePathAsync("qbittorrent");
            serviceLogger = new MusicGQL.Features.Downloads.Services.DownloadLogger(path);
        }
        return serviceLogger;
    }

    public async Task<bool> FinalizeReleaseAsync(
        string artistId,
        string releaseFolderName,
        CancellationToken ct
    )
    {
        var sLogger = await GetLogger();
        try
        {
            var rel = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);
            if (rel == null)
            {
                logger.LogWarning(
                    "[qB Finalize] Release not found in cache {ArtistId}/{Folder}",
                    artistId,
                    releaseFolderName
                );
                return false;
            }

            // Find a torrent whose name looks like the release title
            var torrents = await qb.GetTorrentsAsync(ct);
            if (torrents.Count == 0)
            {
                sLogger.Info("[qB Finalize] No torrents returned by qBittorrent");
                return false;
            }

            var targetName = $"{rel.ArtistName} - {rel.Title}".ToLowerInvariant();
            var candidate = torrents.FirstOrDefault(t =>
                (t.Name ?? string.Empty)
                    .ToLowerInvariant()
                    .Contains(rel.ArtistName.ToLowerInvariant())
                && (t.Name ?? string.Empty)
                    .ToLowerInvariant()
                    .Contains(rel.Title.ToLowerInvariant())
            );
            if (candidate == null)
            {
                sLogger.Warn($"[qB Finalize] No torrent matched name '{targetName}'");
                return false;
            }

            // Guard: only copy when complete; otherwise log and exit
            if (candidate.Progress < 0.999)
            {
                sLogger.Info(
                    $"[qB Finalize] Torrent '{candidate.Name}' not complete yet (progress={candidate.Progress:P0}); skipping copy for now"
                );
                return false;
            }

            // Determine target directory
            var targetDir = rel.ReleasePath;
            if (string.IsNullOrWhiteSpace(targetDir))
            {
                sLogger.Warn("[qB Finalize] Release has no target directory");
                return false;
            }
            Directory.CreateDirectory(targetDir);

            // If qB reports a content path, prefer copying audio files from there instead of moving qB's save path.
            if (
                !string.IsNullOrWhiteSpace(candidate.ContentPath)
                && Directory.Exists(candidate.ContentPath)
            )
            {
                sLogger.Info($"[qB Finalize] Using content path for copy: {candidate.ContentPath}");
                var copied = QbFinalizeCopier.CopyAudioFilesRecursively(
                    candidate.ContentPath!,
                    targetDir,
                    sLogger
                );
                if (copied > 0)
                {
                    sLogger.Info("[qB Finalize] Copy phase completed successfully");
                    return true;
                }
                sLogger.Warn("[qB Finalize] No audio files were copied from content path");
            }

            // Fallback: try save path
            if (
                !string.IsNullOrWhiteSpace(candidate.SavePath)
                && Directory.Exists(candidate.SavePath)
            )
            {
                sLogger.Info($"[qB Finalize] Using save path for copy: {candidate.SavePath}");
                var copied = QbFinalizeCopier.CopyAudioFilesRecursively(
                    candidate.SavePath!,
                    targetDir,
                    sLogger
                );
                if (copied > 0)
                {
                    sLogger.Info("[qB Finalize] Copy phase completed successfully (save path)");
                    return true;
                }
                sLogger.Warn("[qB Finalize] No audio files were copied from save path");
            }

            // As a last resort, attempt setLocation to encourage qBittorrent to move data to release directory
            sLogger.Info($"[qB Finalize] Attempting setLocation to {targetDir}");
            var moved = await qb.SetLocationAsync(candidate.Hash, targetDir, ct);
            if (!moved)
            {
                sLogger.Warn("[qB Finalize] setLocation failed");
                return false;
            }

            sLogger.Info("[qB Finalize] setLocation succeeded");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[qB Finalize] Exception");
            (await GetLogger()).Error($"Exception: {ex.Message}");
            return false;
        }
    }
}
