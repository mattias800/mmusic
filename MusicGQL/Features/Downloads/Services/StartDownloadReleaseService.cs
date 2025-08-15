using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Playlists.Db;
using MusicGQL.Features.Playlists.Subscription;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Writer;
using Path = System.IO.Path;
using MusicGQL.Features.ServerSettings;

namespace MusicGQL.Features.Downloads.Services;

public class StartDownloadReleaseService(
    ServerLibraryCache cache,
    ServerLibraryJsonWriter writer,
    ILogger<StartDownloadReleaseService> logger,
    Features.Import.Services.MusicBrainzImportService mbImport,
    Features.Import.Services.LibraryReleaseImportService releaseImporter,
    HotChocolate.Subscriptions.ITopicEventSender eventSender,
    IDbContextFactory<EventDbContext> dbFactory,
    DownloadCancellationService cancellationService,
    ServerSettingsAccessor serverSettingsAccessor,
    MusicGQL.Features.External.Downloads.DownloadProviderCatalog providers,
    MusicGQL.Features.External.Downloads.Sabnzbd.SabnzbdFinalizeService sabFinalize,
    CurrentDownloadStateService currentDownloadState,
    DownloadHistoryService downloadHistory
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

        // Compute allowed track counts on-demand (do not persist in release.json)
        List<int> allowedOfficialCounts = new();
        List<int> allowedOfficialDigitalCounts = new();
        try
        {
            var artist = await cache.GetArtistByIdAsync(artistId);
            var mbArtistId = artist?.JsonArtist.Connections?.MusicBrainzArtistId;
            var mbReleaseGroupId = release.JsonRelease?.Connections?.MusicBrainzReleaseGroupId;
            if (!string.IsNullOrWhiteSpace(mbReleaseGroupId))
            {
                (allowedOfficialCounts, allowedOfficialDigitalCounts) = await mbImport.GetPossibleTrackCountsForReleaseGroupAsync(mbReleaseGroupId!);
            }
            else if (!string.IsNullOrWhiteSpace(mbArtistId) && !string.IsNullOrWhiteSpace(releaseTitle))
            {
                var rgs = await mbImport.GetArtistReleaseGroupsAsync(mbArtistId!);
                var candidates = rgs
                    .Where(rg => AreTitlesEquivalent(rg.Title, releaseTitle))
                    .Where(rg => !(rg.SecondaryTypes?.Any(t => t.Equals("Demo", StringComparison.OrdinalIgnoreCase)) ?? false))
                    .ToList();
                if (candidates.Count == 0)
                {
                    candidates = rgs
                        .Where(rg => (rg.Title ?? string.Empty).IndexOf(releaseTitle, StringComparison.OrdinalIgnoreCase) >= 0)
                        .Where(rg => !(rg.SecondaryTypes?.Any(t => t.Equals("Demo", StringComparison.OrdinalIgnoreCase)) ?? false))
                        .ToList();
                }
                var match = candidates
                    .OrderByDescending(rg => string.Equals(rg.PrimaryType, "Album", StringComparison.OrdinalIgnoreCase))
                    .ThenBy(rg => SafeDateKey(rg.FirstReleaseDate))
                    .FirstOrDefault();
                if (match is not null)
                {
                    (allowedOfficialCounts, allowedOfficialDigitalCounts) = await mbImport.GetPossibleTrackCountsForReleaseGroupAsync(match.Id);
                }
            }
        }
        catch (Exception enrichEx)
        {
            logger.LogDebug(enrichEx, "[StartDownload] Skipped on-demand track count computation");
        }

        // Set status to Searching before starting
        await cache.UpdateReleaseDownloadStatus(
            artistId,
            releaseFolderName,
            CachedReleaseDownloadStatus.Searching
        );

        var token = cancellationService.CreateFor(artistId, releaseFolderName, cancellationToken);
        var ok = false;
        string? providerName = null; // Declare at broader scope for use in final success/failure determination
        var providerList = providers.Providers.ToList();
        var totalProviders = providerList.Count;
        
        // Initialize the enhanced download history item with Idle state
        var enhancedHistoryItem = new EnhancedDownloadHistoryItem
        {
            Id = $"{artistId}|{releaseFolderName}",
            TimestampUtc = DateTime.UtcNow,
            ArtistId = artistId,
            ReleaseFolderName = releaseFolderName,
            ArtistName = artistName,
            ReleaseTitle = releaseTitle,
            CurrentState = DownloadState.Idle,
            FinalResult = DownloadResult.NoResultYet,
            ProviderUsed = null,
            StateStartTime = DateTime.UtcNow
        };
        
        downloadHistory.AddEnhanced(enhancedHistoryItem);
        
        // Set initial searching status with provider info
        currentDownloadState.Set(new DownloadProgress
        {
            ArtistId = artistId,
            ReleaseFolderName = releaseFolderName,
            ArtistName = artistName,
            ReleaseTitle = releaseTitle,
            Status = DownloadStatus.Searching,
            TotalProviders = totalProviders,
            CurrentProviderIndex = 0,
            CurrentProvider = null
        });
        
        // Update state to Searching
        downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Searching, "Starting search across download providers");
        
        for (int i = 0; i < providerList.Count; i++)
        {
            var provider = providerList[i];
            providerName = provider.GetType().Name; // Assign to the broader scope variable
            
            try
            {
                // Update current provider info
                currentDownloadState.Set(new DownloadProgress
                {
                    ArtistId = artistId,
                    ReleaseFolderName = releaseFolderName,
                    ArtistName = artistName,
                    ReleaseTitle = releaseTitle,
                    Status = DownloadStatus.Searching,
                    TotalProviders = totalProviders,
                    CurrentProviderIndex = i + 1,
                    CurrentProvider = providerName
                });
                
                // Update state to show current provider being tried
                downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Searching, $"Trying provider {providerName} ({i + 1}/{totalProviders})");
                
                logger.LogInformation("[StartDownload] Trying provider {Provider} ({Index}/{Total}) for {Artist}/{Folder}", 
                    providerName, i + 1, totalProviders, artistId, releaseFolderName);
                
                ok = await provider.TryDownloadReleaseAsync(
                    artistId,
                    releaseFolderName,
                    artistName,
                    releaseTitle,
                    targetDir,
                    allowedOfficialCounts,
                    allowedOfficialDigitalCounts,
                    token
                );
                
                if (ok)
                {
                    logger.LogInformation("[StartDownload] Provider {Provider} reported success for {Artist}/{Folder}", providerName, artistId, releaseFolderName);
                    
                    // Update state to Downloading when provider reports success
                    downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Downloading, $"Provider {providerName} reported success, queuing download");
                    
                    // Update the existing enhanced history item with provider information
                    var existingItem = downloadHistory.GetEnhanced(artistId, releaseFolderName);
                    if (existingItem != null)
                    {
                        var updatedItem = existingItem with
                        {
                            ProviderUsed = providerName,
                            CurrentState = DownloadState.Downloading
                        };
                        // Note: We can't directly update the item, but the state update above will handle this
                    }
                    
                    break;
                }
                else
                {
                    logger.LogInformation("[StartDownload] Provider {Provider} reported no result for {Artist}/{Folder}", providerName, artistId, releaseFolderName);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[StartDownload] Provider {Provider} failed", providerName);
                
                // Update state to show provider failed
                downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Searching, $"Provider {providerName} failed: {ex.Message}");
                
                // Update state to Finished
                downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Finished, $"Download process failed due to provider {providerName} failure");
            }
        }
        
        if (!ok)
        {
            var msg = $"No suitable download found for {artistName} - {releaseTitle}";
            logger.LogWarning("[StartDownload] {Message}", msg);

            // Update download result to show no search result
            downloadHistory.UpdateResult(artistId, releaseFolderName, DownloadResult.NoSearchResult, msg);
            
            // Update state to Finished
            downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Finished, "Download process failed - no suitable download found");

            await cache.UpdateReleaseDownloadStatus(
                artistId,
                releaseFolderName,
                CachedReleaseDownloadStatus.NotFound
            );

            return (false, "No suitable download found");
        }

        // Phase 2: Metadata refresh and track count validation
        logger.LogInformation("[StartDownload] Phase 1 complete - download queued. Starting Phase 2: metadata refresh and track count validation for {ArtistId}/{ReleaseFolder}", artistId, releaseFolderName);
        
        // Track the start time of Phase 2 for performance monitoring
        var phase2StartTime = DateTime.UtcNow;
        
        // Update state to ImportingFiles
        downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.ImportingFiles, "Starting file import and metadata refresh");
        
        // Validate that we have a valid target directory before proceeding
        if (string.IsNullOrWhiteSpace(targetDir) || !Directory.Exists(targetDir))
        {
            var error = $"Target directory is invalid or does not exist: {targetDir}";
            logger.LogError("[StartDownload] {Error}", error);
            
            // Update download result to show failure
            downloadHistory.UpdateResult(artistId, releaseFolderName, DownloadResult.UnknownError, error);
            
            // Update state to Finished
            downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Finished, "Download process failed due to invalid target directory");
            
            return (false, error);
        }

        // Opportunistic finalize from SAB completed folder in case watcher missed events
        try
        {
            // Update state to show SAB finalization starting
            downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.ImportingFiles, "Starting SAB finalization process");
            
            var finalized = await sabFinalize.FinalizeReleaseAsync(artistId, releaseFolderName, token);
            if (finalized)
            {
                logger.LogInformation("[StartDownload] SAB finalize moved audio for {ArtistId}/{Folder}", artistId, releaseFolderName);
                
                // Update state to show SAB finalization completed
                downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.ImportingFiles, "SAB finalization completed successfully");
            }
            else
            {
                // Update state to show SAB finalization didn't complete
                downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.ImportingFiles, "SAB finalization did not complete (files may still be processing)");
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "[StartDownload] SAB finalize attempt failed (non-fatal)");
            
            // Update state to show SAB finalization failed
            downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.ImportingFiles, "SAB finalization failed (non-fatal)");
            
            // Update state to Finished
            downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Finished, "Download process completed with SAB finalization failure (non-fatal)");
        }

        var releaseJsonPath = Path.Combine(targetDir, "release.json");
        logger.LogInformation("[StartDownload] Updating JSON at {Path}", releaseJsonPath);

        if (File.Exists(releaseJsonPath))
        {
            try
            {
                // Update state to show JSON update starting
                downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.ImportingFiles, "Starting JSON update with audio file paths");
                
                logger.LogDebug("[StartDownload] release.json exists. Enumerating audio files for injection...");
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
                        // Build map from leading track number in filename -> filename
                        int ExtractLeadingNumber(string? name)
                        {
                            if (string.IsNullOrWhiteSpace(name)) return -1;
                            var span = name.AsSpan();
                            int pos = 0;
                            while (pos < span.Length && !char.IsDigit(span[pos])) pos++;
                            int start = pos;
                            while (pos < span.Length && char.IsDigit(span[pos])) pos++;
                            if (pos > start && int.TryParse(span.Slice(start, pos - start), out var n))
                            {
                                // Normalize 3+ digit disc+track encodings (e.g., 103 -> 3)
                                if (n > 99)
                                {
                                    var lastTwo = n % 100;
                                    if (lastTwo > 0) return lastTwo;
                                }
                                return n;
                            }
                            return -1;
                        }

                        var byTrackNo = new Dictionary<int, string>();
                        foreach (var f in audioFiles)
                        {
                            var n = ExtractLeadingNumber(f);
                            if (n > 0 && !byTrackNo.ContainsKey(n)) byTrackNo[n] = f;
                        }

                        foreach (var t in rel.Tracks)
                        {
                            if (byTrackNo.TryGetValue(t.TrackNumber, out var fname))
                            {
                                t.AudioFilePath = "./" + fname;
                            }
                        }
                    }
                );

                logger.LogInformation("[StartDownload] Updated release.json with audio file paths");

                // Update state to show JSON update completed
                downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.ImportingFiles, "JSON update completed, refreshing cache");

                // Reload just this release into cache so it reflects new JSON (preserves transient availability)
                logger.LogInformation(
                    "[StartDownload] Refreshing release in cache after JSON update..."
                );
                await cache.UpdateReleaseFromJsonAsync(artistId, releaseFolderName);

                // Update state to show cache refresh completed
                downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.ImportingFiles, "Cache refresh completed, updating media availability status");

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
                logger.LogInformation("[StartDownload] Marked {Count} tracks as Available", relAfterCount);

                // Update state to show media availability status updates completed
                downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.ImportingFiles, "Media availability status updates completed, linking playlist items");

                // Backfill playlist items that should now reference these tracks
                try
                {
                    await LinkPlaylistItemsForReleaseAsync(artistId, releaseFolderName);
                }
                catch (Exception backfillEx)
                {
                    logger.LogWarning(
                        backfillEx,
                        "[StartDownload] Failed linking playlist items for {ArtistId}/{Folder}",
                        artistId,
                        releaseFolderName
                    );
                    
                    // Update state to show playlist linking failed
                    downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.ImportingFiles, "Playlist linking failed (non-fatal)");
                    
                    // Update state to Finished
                    downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Finished, "Download process completed with playlist linking failure (non-fatal)");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[StartDownload] JSON update failed for {ArtistId}/{Folder}", artistId, releaseFolderName);
                
                // Update state to show JSON update failed
                downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.ImportingFiles, "JSON update failed");
                
                // Update state to Finished
                downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Finished, "Download process failed during JSON update");
            }
        }
        else
        {
            logger.LogWarning("[StartDownload] release.json not found at path: {Path}. Skipping JSON update.", releaseJsonPath);
            
            // Update state to show no JSON update needed
            downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.ImportingFiles, "No release.json found, skipping JSON update");
        }

        await cache.UpdateReleaseDownloadStatus(
            artistId,
            releaseFolderName,
            CachedReleaseDownloadStatus.Idle
        );

        // Auto-refresh metadata now that audio exists
        bool metadataRefreshSuccess = false;
        string? metadataError = null;
        
        // Update state to MatchingRelease
        downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.MatchingRelease, "Starting metadata refresh from MusicBrainz");
        
        try
        {
            var rel = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);
            var artist = await cache.GetArtistByIdAsync(artistId);
            var mbArtistId = artist?.JsonArtist.Connections?.MusicBrainzArtistId;
            if (!string.IsNullOrWhiteSpace(mbArtistId) && rel != null)
            {
                // Update state to show MusicBrainz search starting
                downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.MatchingRelease, "Searching MusicBrainz for matching release groups");
                
                var rgs = await mbImport.GetArtistReleaseGroupsAsync(mbArtistId!);

                // Prefer RGs whose titles are equivalent, exclude demos, prefer Album primary type, then earliest date
                var candidates = rgs
                    .Where(rg => AreTitlesEquivalent(rg.Title, rel.Title ?? string.Empty))
                    .Where(rg => !(rg.SecondaryTypes?.Any(t => t.Equals("Demo", StringComparison.OrdinalIgnoreCase)) ?? false))
                    .ToList();

                if (candidates.Count == 0)
                {
                    // Fallback: loose contains check
                    candidates = rgs
                        .Where(rg => (rg.Title ?? string.Empty).IndexOf(rel.Title ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0)
                        .Where(rg => !(rg.SecondaryTypes?.Any(t => t.Equals("Demo", StringComparison.OrdinalIgnoreCase)) ?? false))
                        .ToList();
                }

                var match = candidates
                    .OrderByDescending(rg => string.Equals(rg.PrimaryType, "Album", StringComparison.OrdinalIgnoreCase))
                    .ThenBy(rg => SafeDateKey(rg.FirstReleaseDate))
                    .FirstOrDefault();

                if (match is not null)
                {
                    // Update state to show release group match found
                    downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.MatchingRelease, $"Found matching release group: {match.Title} ({match.Id})");
                    
                    // Rebuild using this RG; builder will pick the best matching release considering local audio files
                    var importResult = await releaseImporter.ImportReleaseGroupInPlaceAsync(
                        match.Id,
                        match.Title,
                        match.PrimaryType,
                        Path.GetDirectoryName(rel.ReleasePath) ?? Path.Combine((await serverSettingsAccessor.GetAsync()).LibraryPath, artistId),
                        artistId,
                        rel.FolderName
                    );
                    if (importResult.Success)
                    {
                        // Update state to show import completed successfully
                        downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.MatchingRelease, "Release import completed successfully");
                        
                        await cache.UpdateReleaseFromJsonAsync(artistId, releaseFolderName);
                        
                        // Update state to ValidatingTracks
                        downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.ValidatingTracks, "Starting track count validation");
                        
                        // Verify that the refreshed release has tracks that match our audio files
                        var refreshedRelease = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);
                        if (refreshedRelease?.Tracks != null)
                        {
                            var audioFiles = Directory
                                .GetFiles(targetDir)
                                .Where(f => new[] { ".mp3", ".flac", ".wav", ".m4a", ".ogg" }.Contains(
                                    Path.GetExtension(f).ToLowerInvariant()
                                ))
                                .ToList();
                            
                            var trackCount = refreshedRelease.Tracks.Count;
                            var audioFileCount = audioFiles.Count;
                            
                            logger.LogInformation("[StartDownload] Track count validation: MusicBrainz release has {TrackCount} tracks, found {AudioFileCount} audio files in {TargetDir}", 
                                trackCount, audioFileCount, targetDir);
                            
                            if (trackCount == audioFileCount)
                            {
                                metadataRefreshSuccess = true;
                                logger.LogInformation("[StartDownload] Metadata refresh successful - track count matches: {TrackCount} tracks, {AudioFileCount} audio files", trackCount, audioFileCount);
                                
                                // Update state to show track validation successful
                                downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.ValidatingTracks, $"Track validation successful: {trackCount} tracks match {audioFileCount} audio files");
                            }
                            else
                            {
                                metadataError = $"Track count mismatch: expected {trackCount} tracks, found {audioFileCount} audio files";
                                logger.LogWarning("[StartDownload] {Error}", metadataError);
                                
                                // Update state to show track validation failed
                                downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.ValidatingTracks, $"Track validation failed: {metadataError}");
                                
                                // Log additional diagnostic information
                                if (audioFiles.Count > 0)
                                {
                                    var audioFileNames = audioFiles.Select(Path.GetFileName).Take(5).ToList();
                                    logger.LogWarning("[StartDownload] Audio files found: {AudioFiles}...", string.Join(", ", audioFileNames));
                                }
                                
                                if (refreshedRelease.Tracks.Count > 0)
                                {
                                    var trackNames = refreshedRelease.Tracks.Take(5).Select(t => t.Title).ToList();
                                    logger.LogWarning("[StartDownload] Expected tracks: {Tracks}...", string.Join(", ", trackNames));
                                }
                            }
                        }
                        else
                        {
                            metadataError = "Refreshed release has no tracks";
                            logger.LogWarning("[StartDownload] {Error}", metadataError);
                        }
                        
                        // Update state to show metadata events being published
                        downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.ValidatingTracks, "Publishing metadata update events");
                        
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

                            // Centralized release and artist notifications
                            await eventSender.SendAsync(
                                ServerLibrary.Subscription.LibrarySubscription.LibraryReleaseUpdatedTopic(
                                    artistId,
                                    releaseFolderName
                                ),
                                new ServerLibrary.Release(updated)
                            );
                            await eventSender.SendAsync(
                                ServerLibrary.Subscription.LibrarySubscription.LibraryArtistReleaseUpdatedTopic(
                                    artistId
                                ),
                                new ServerLibrary.Release(updated)
                            );
                            
                            // Update state to show metadata events published successfully
                            downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.ValidatingTracks, "Metadata update events published successfully");
                        }
                    }
                    else
                    {
                        metadataError = $"Metadata import failed: {importResult.ErrorMessage}";
                        logger.LogWarning("[StartDownload] {Error}", metadataError);
                        
                        // Update state to show import failed
                        downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.MatchingRelease, $"Release import failed: {metadataError}");
                    }
                }
                else
                {
                    metadataError = "No matching release group found in MusicBrainz";
                    logger.LogWarning("[StartDownload] {Error}", metadataError);
                    
                    // Update state to show no matching release group found
                    downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.MatchingRelease, "No matching release group found in MusicBrainz");
                }
            }
            else
            {
                metadataError = "No MusicBrainz artist ID or release found for metadata refresh";
                logger.LogWarning("[StartDownload] {Error}", metadataError);
                
                // Update state to show no MusicBrainz data available
                downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.MatchingRelease, "No MusicBrainz artist ID or release found for metadata refresh");
            }
        }
        catch (Exception ex)
        {
            metadataError = $"Metadata refresh failed: {ex.Message}";
            logger.LogWarning(ex, "[StartDownload] Auto-refresh after download failed");
            
            // Update state to show metadata refresh failed
            downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.MatchingRelease, $"Metadata refresh failed: {ex.Message}");
            
            // Update state to Finished
            downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Finished, "Download process failed during metadata refresh");
        }

        // Phase 2: Final success/failure determination based on metadata refresh and track count validation
        downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.ValidatingTracks, "Determining final download result");
        
        if (metadataRefreshSuccess)
        {
            // Update the download result to show final success
            downloadHistory.UpdateResult(artistId, releaseFolderName, DownloadResult.Success);
            
            // Update state to Finished
            downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Finished, "Download process completed successfully");
            
            logger.LogInformation("[StartDownload] Download completed successfully for {ArtistId}/{ReleaseFolder} after metadata validation", artistId, releaseFolderName);
        }
        else
        {
            // Determine the specific failure reason
            var failureResult = metadataError switch
            {
                var msg when msg?.Contains("Track count mismatch") == true => DownloadResult.TrackCountMismatch,
                var msg when msg?.Contains("No matching release group") == true => DownloadResult.NoMatchingReleases,
                var msg when msg?.Contains("Metadata refresh failed") == true => DownloadResult.MetadataRefreshFailed,
                _ => DownloadResult.UnknownError
            };
            
            // Update the download result to show final failure
            downloadHistory.UpdateResult(artistId, releaseFolderName, failureResult, metadataError);
            
            // Update state to Finished
            downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Finished, "Download process failed during metadata validation");
            
            logger.LogWarning("[StartDownload] Download failed for {ArtistId}/{ReleaseFolder} after metadata validation: {Error}", artistId, releaseFolderName, metadataError);
        }

        // Log Phase 2 completion time
        var phase2Duration = DateTime.UtcNow - phase2StartTime;
        logger.LogInformation("[StartDownload] Phase 2 completed in {Duration:g} for {ArtistId}/{ReleaseFolder}", phase2Duration, artistId, releaseFolderName);

        // Update state to show final cleanup starting
        downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Finished, "Starting final cleanup and completion");

        // Final state transition to Finished
        downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Finished, "Download process completed");

        // Finished
        logger.LogInformation("[StartDownload] Done for {ArtistId}/{ReleaseFolder}", artistId, releaseFolderName);
        return (true, null);
    }

    private async Task LinkPlaylistItemsForReleaseAsync(string artistId, string releaseFolderName)
    {
        var rel = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);
        if (rel == null || rel.Tracks == null || rel.Tracks.Count == 0)
            return;

        await using var db = await dbFactory.CreateDbContextAsync();

        // Candidates: items already linked to this artist but missing track mapping
        var candidates = await db
            .Set<DbPlaylistItem>()
            .Where(i =>
                i.LocalArtistId == artistId
                && (i.LocalReleaseFolderName == null || i.LocalReleaseFolderName == releaseFolderName)
                && i.LocalTrackNumber == null
                && i.SongTitle != null
            )
            .ToListAsync();

        if (candidates.Count == 0)
            return;

        var updated = new List<DbPlaylistItem>();
        foreach (var item in candidates)
        {
            // If item has a release title, require it to match the release title to reduce false positives
            if (!string.IsNullOrWhiteSpace(item.ReleaseTitle)
                && !AreTitlesEquivalent(item.ReleaseTitle!, rel.Title ?? string.Empty))
            {
                continue;
            }

            var title = item.SongTitle ?? string.Empty;
            var match = rel.Tracks
                .Where(t => !string.IsNullOrWhiteSpace(t.Title))
                .FirstOrDefault(t => AreTitlesEquivalent(t.Title!, title));

            if (match != null)
            {
                item.LocalReleaseFolderName = releaseFolderName;
                item.LocalTrackNumber = match.TrackNumber;
                updated.Add(item);
            }
        }

        if (updated.Count == 0)
            return;

        await db.SaveChangesAsync();

        // Notify subscribers for each updated item
        foreach (var item in updated)
        {
            try
            {
                await eventSender.SendAsync(
                    PlaylistSubscription.PlaylistItemUpdatedTopic(item.PlaylistId),
                    new PlaylistSubscription.PlaylistItemUpdatedMessage(item.PlaylistId, item.Id)
                );
            }
            catch { /* best effort */ }
        }
    }

    private static string NormalizeTitle(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var s = input
            .Replace("’", "'")
            .Replace("“", "\"")
            .Replace("”", "\"");
        var builder = new System.Text.StringBuilder(s.Length);
        foreach (var ch in s)
        {
            if (char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
        }
        var normalized = System.Text.RegularExpressions.Regex.Replace(builder.ToString(), "\\s+", " ").Trim();
        return normalized;
    }

    private static string StripParentheses(string input)
    {
        return System.Text.RegularExpressions.Regex.Replace(input, "\\(.*?\\)", string.Empty).Trim();
    }

    private static bool AreTitlesEquivalent(string a, string b)
    {
        var na = NormalizeTitle(a);
        var nb = NormalizeTitle(b);
        if (na.Equals(nb, StringComparison.Ordinal)) return true;

        var npa = NormalizeTitle(StripParentheses(a));
        var npb = NormalizeTitle(StripParentheses(b));
        return npa.Equals(npb, StringComparison.Ordinal);
    }

    private static int SafeDateKey(string? iso)
    {
        if (string.IsNullOrWhiteSpace(iso)) return int.MaxValue;
        var parts = iso.Split('-');
        int y = parts.Length > 0 && int.TryParse(parts[0], out var yy) ? yy : 9999;
        int m = parts.Length > 1 && int.TryParse(parts[1], out var mm) ? mm : 12;
        int d = parts.Length > 2 && int.TryParse(parts[2], out var dd) ? dd : 31;
        return y * 372 + m * 31 + d;
    }
}
