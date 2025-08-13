using MusicGQL.Features.ArtistImportQueue.Services;
using MusicGQL.Features.Downloads.Services;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerSettings;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Services;

public class ArtistDeletionService(
    ServerLibraryCache cache,
    DownloadQueueService downloadQueue,
    DownloadCancellationService downloadCancellation,
    CurrentDownloadStateService currentDownload,
    ArtistImportQueueService artistImportQueue,
    ILogger<ArtistDeletionService> logger,
    ServerSettingsAccessor serverSettingsAccessor
)
{
    public async Task<(bool Success, string? ErrorMessage)> DeleteArtistCompletelyAsync(string artistId)
    {
        try
        {
            logger.LogInformation("[ArtistDeletion] Begin delete for {ArtistId}", artistId);

            // 1) Cancel active download for this artist (best effort)
            var cancelled = downloadCancellation.CancelActiveForArtist(artistId);
            if (cancelled)
            {
                currentDownload.SetError("Cancelled due to artist deletion");
                currentDownload.Reset();
            }

            // 2) Remove queued downloads for this artist
            var removedFromDlQueue = downloadQueue.RemoveAllForArtist(artistId);
            logger.LogInformation("[ArtistDeletion] Removed {Count} queued downloads for {ArtistId}", removedFromDlQueue, artistId);

            // 3) Remove artist import queue items referencing this artist (refresh jobs)
            // This service does not currently expose a per-artist purge; best effort by rebuilding the queue
            try
            {
                PurgeArtistImportQueueForArtist(artistId);
            }
            catch { }

            // 4) Delete the artist directory from disk
            var lib = await serverSettingsAccessor.GetAsync();
            var dir = Path.Combine(lib.LibraryPath, artistId);
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }

            // 5) Refresh cache so the artist is removed from memory/state
            await cache.UpdateCacheAsync();

            logger.LogInformation("[ArtistDeletion] Completed delete for {ArtistId}", artistId);
            return (true, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[ArtistDeletion] Failed to delete artist {ArtistId}", artistId);
            return (false, ex.Message);
        }
    }

    private void PurgeArtistImportQueueForArtist(string artistId)
    {
        // Rebuild without items that target this artist id (RefreshReleaseMetadata jobs)
        var snapshot = artistImportQueue.Snapshot();
        int removed = 0;
        foreach (var item in snapshot.Items)
        {
            if (string.Equals(item.LocalArtistId, artistId, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(item.QueueKey))
                {
                    try { if (artistImportQueue.TryRemove(item.QueueKey!)) removed++; } catch { }
                }
            }
        }
        if (removed > 0)
        {
            logger.LogInformation("[ArtistDeletion] Removed {Count} artist import jobs for {ArtistId}", removed, artistId);
        }
    }
}


