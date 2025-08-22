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
    DownloadHistoryService downloadHistory,
    DownloadLogPathProvider logPathProvider
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

        // Initialize per-release log (orchestrator-level)
        IDownloadLogger relLogger = new NullDownloadLogger();
        DownloadLogger? relLoggerImpl = null;
        try
        {
            var logPath = await logPathProvider.GetReleaseLogFilePathAsync(artistName, releaseTitle, cancellationToken);
            if (!string.IsNullOrWhiteSpace(logPath))
            {
                relLoggerImpl = new DownloadLogger(logPath!);
                relLogger = relLoggerImpl;
            }
        }
        catch { }
        try { relLogger.Info($"[Orchestrator] Start for {artistName} - {releaseTitle}"); } catch { }

        logger.LogInformation(
            "[StartDownload] Resolved targetDir={TargetDir}, artistName='{Artist}', releaseTitle='{Title}'",
            targetDir,
            artistName,
            releaseTitle
        );
        
        // Log the artist name source for debugging
        var currentArtistName = (await cache.GetArtistByIdAsync(artistId))?.JsonArtist.Name ?? "Unknown";
        if (artistName != currentArtistName)
        {
            logger.LogInformation(
                "[StartDownload] Using historical artist name '{HistoricalName}' (from release.json) instead of current name '{CurrentName}' for search",
                artistName,
                currentArtistName
            );
        }

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
        try
        {
            if (allowedOfficialCounts.Count > 0)
                relLogger.Info($"[Orchestrator] Allowed official track counts: {string.Join(", ", allowedOfficialCounts)}");
            if (allowedOfficialDigitalCounts.Count > 0)
                relLogger.Info($"[Orchestrator] Allowed DIGITAL track counts: {string.Join(", ", allowedOfficialDigitalCounts)}");
        }
        catch { }

        // Set status to Searching before starting
        await cache.UpdateReleaseDownloadStatus(
            artistId,
            releaseFolderName,
            CachedReleaseDownloadStatus.Searching
        );
        try { relLogger.Info("[Orchestrator] Set download status = Searching"); } catch { }

        var token = cancellationService.CreateFor(artistId, releaseFolderName, cancellationToken);
        var ok = false; // last provider result
        var anyAccepted = false; // whether any provider accepted/started a download
        string? providerName = null; // Declare at broader scope for use in logging and state
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
                try { relLogger.Info($"[Orchestrator] Trying provider {providerName} ({i + 1}/{totalProviders})"); } catch { }
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
                    anyAccepted = true;
                    if (providerName == "ProwlarrDownloadProvider")
                    {
                        logger.LogInformation("[StartDownload] Provider {Provider} accepted job handoff (SAB/qBittorrent). Will wait for files to appear in library.", providerName);
                        downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Downloading, $"{providerName} accepted job handoff; awaiting files");
                        try { relLogger.Info($"[Orchestrator] {providerName} accepted job handoff; awaiting files"); } catch { }
                    }
                    else if (providerName == "SoulSeekDownloadProvider")
                    {
                        logger.LogInformation("[StartDownload] Provider {Provider} completed download flow", providerName);
                        downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Downloading, $"{providerName} completed download");
                        try { relLogger.Info($"[Orchestrator] {providerName} completed download"); } catch { }
                    }
                    else
                    {
                        logger.LogInformation("[StartDownload] Provider {Provider} succeeded", providerName);
                        downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Downloading, $"{providerName} succeeded");
                        try { relLogger.Info($"[Orchestrator] {providerName} succeeded"); } catch { }
                    }
                    
                    // Continue to next provider to exhaust all download services
                }
                else
                {
                    logger.LogInformation("[StartDownload] Provider {Provider} found no suitable result; moving to next", providerName);
                    downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Searching, $"{providerName} found no result; trying next provider");
                    try { relLogger.Info($"[Orchestrator] Provider {providerName} returned no result"); } catch { }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[StartDownload] Provider {Provider} failed", providerName);
                
                // Update state to show provider failed
                downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Searching, $"Provider {providerName} failed: {ex.Message}");
                
                // Continue to next provider on failure
                try { relLogger.Info($"[Orchestrator] Provider {providerName} failed: {ex.Message}"); } catch { }
            }
        }
        
        if (!anyAccepted)
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
        try { relLogger.Info("[Orchestrator] Phase 2 start: finalize + JSON update + metadata validation"); } catch { }
        
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
                try { relLogger.Info("[Orchestrator] SAB finalize completed successfully"); } catch { }
                
                // Update state to show SAB finalization completed
                downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.ImportingFiles, "SAB finalization completed successfully");
            }
            else
            {
                try { relLogger.Warn("[Orchestrator] SAB finalize did not complete (files may still be processing)"); } catch { }
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

        // Before any JSON/meta work, check if any audio files are present yet
        var currentAudioCount = 0;
        try
        {
            currentAudioCount = Directory
                .GetFiles(targetDir)
                .Count(f => new[] { ".mp3", ".flac", ".wav", ".m4a", ".ogg" }
                    .Contains(Path.GetExtension(f).ToLowerInvariant()));
        }
        catch { }

        if (currentAudioCount == 0)
        {
            // Keep the download in progress until files arrive (via SAB watcher/history scanner or background providers)
            logger.LogInformation("[StartDownload] No audio files present yet at {TargetDir}. Keeping in progress and awaiting files from providers.", targetDir);
            downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Downloading, "Awaiting files from providers");
            await cache.UpdateReleaseDownloadStatus(artistId, releaseFolderName, CachedReleaseDownloadStatus.Downloading);
            return (true, null);
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
                try { relLogger.Info($"[Orchestrator] Found {audioFiles.Count} audio files in target directory"); } catch { }

                await writer.UpdateReleaseAsync(
                    artistId,
                    releaseFolderName,
                    rel =>
                    {
                        // Extract disc/track numbers from file names
                        static (int disc, int track) ExtractDiscTrack(string? name)
                        {
                            int disc = 1;
                            int track = -1;
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(name))
                                {
                                    var lower = name!.ToLowerInvariant();
                                    var m = System.Text.RegularExpressions.Regex.Match(lower, @"\b(?:cd|disc|disk|digital\s*media)\s*(\d+)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                    if (m.Success && int.TryParse(m.Groups[1].Value, out var d)) disc = d;
                                    var m2 = System.Text.RegularExpressions.Regex.Match(name, @"-\s*(\d{1,3})\s*-", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                    if (m2.Success && int.TryParse(m2.Groups[1].Value, out var t2)) track = t2;
                                    if (track < 0)
                                    {
                                        var span = name.AsSpan();
                                        int pos = 0;
                                        while (pos < span.Length && !char.IsDigit(span[pos])) pos++;
                                        int start = pos;
                                        while (pos < span.Length && char.IsDigit(span[pos])) pos++;
                                        if (pos > start && int.TryParse(span.Slice(start, pos - start), out var n))
                                        {
                                            track = n > 99 ? (n % 100 == 0 ? n : n % 100) : n;
                                        }
                                    }
                                }
                            }
                            catch { }
                            return (disc, track);
                        }

                        var byDiscTrack = new Dictionary<int, Dictionary<int, string>>();
                        foreach (var f in audioFiles)
                        {
                            var (disc, track) = ExtractDiscTrack(f);
                            if (track <= 0) continue;
                            if (!byDiscTrack.TryGetValue(disc, out var inner))
                            {
                                inner = new Dictionary<int, string>();
                                byDiscTrack[disc] = inner;
                            }
                            if (!inner.ContainsKey(track)) inner[track] = f;
                        }

                        // Update discs when present
                        if (rel.Discs is { Count: > 0 })
                        {
                            foreach (var d in rel.Discs)
                            {
                                if (d.Tracks == null) continue;
                                var discNum = d.DiscNumber > 0 ? d.DiscNumber : 1;
                                if (byDiscTrack.TryGetValue(discNum, out var inner))
                                {
                                    foreach (var t in d.Tracks)
                                    {
                                        if (t.TrackNumber > 0 && inner.TryGetValue(t.TrackNumber, out var fname))
                                        {
                                            t.AudioFilePath = "./" + System.IO.Path.GetFileName(fname);
                                        }
                                    }
                                }
                            }
                        }

                        // Update flattened tracks for compatibility
                        if (rel.Tracks is { Count: > 0 })
                        {
                            foreach (var t in rel.Tracks)
                            {
                                var discNum = t.DiscNumber ?? 1;
                                if (byDiscTrack.TryGetValue(discNum, out var inner) && inner.TryGetValue(t.TrackNumber, out var fname))
                                {
                                    t.AudioFilePath = "./" + System.IO.Path.GetFileName(fname);
                                }
                            }
                        }
                    }
                );

                logger.LogInformation("[StartDownload] Updated release.json with audio file paths");
                try { relLogger.Info("[Orchestrator] Updated release.json with audio file paths"); } catch { }

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
                try { relLogger.Info($"[Orchestrator] Marked {relAfterCount} tracks as Available"); } catch { }

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
        try { relLogger.Info("[Orchestrator] Set download status = Idle"); } catch { }

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
                try { relLogger.Info($"[Orchestrator] Matched MusicBrainz RG: {match.Title} ({match.Id})"); } catch { }
                
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
                                try { relLogger.Info($"[Orchestrator] Metadata refresh OK — track count matches: {trackCount}"); } catch { }
                                
                                // Update state to show track validation successful
                                downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.ValidatingTracks, $"Track validation successful: {trackCount} tracks match {audioFileCount} audio files");
                            }
                            else
                            {
                                metadataError = $"Track count mismatch: expected {trackCount} tracks, found {audioFileCount} audio files";
                                logger.LogWarning("[StartDownload] {Error}", metadataError);
                                try { relLogger.Warn($"[Orchestrator] {metadataError}"); } catch { }
                                
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
                        try { relLogger.Warn($"[Orchestrator] {metadataError}"); } catch { }
                        
                        // Update state to show import failed
                        downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.MatchingRelease, $"Release import failed: {metadataError}");
                    }
                }
                else
                {
                    metadataError = "No matching release group found in MusicBrainz";
                    logger.LogWarning("[StartDownload] {Error}", metadataError);
                    try { relLogger.Warn($"[Orchestrator] {metadataError}"); } catch { }
                    
                    // Update state to show no matching release group found
                    downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.MatchingRelease, "No matching release group found in MusicBrainz");
                }
            }
            else
            {
                metadataError = "No MusicBrainz artist ID or release found for metadata refresh";
                logger.LogWarning("[StartDownload] {Error}", metadataError);
                try { relLogger.Warn($"[Orchestrator] {metadataError}"); } catch { }
                
                // Update state to show no MusicBrainz data available
                downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.MatchingRelease, "No MusicBrainz artist ID or release found for metadata refresh");
            }
        }
        catch (Exception ex)
        {
            metadataError = $"Metadata refresh failed: {ex.Message}";
            logger.LogWarning(ex, "[StartDownload] Auto-refresh after download failed");
            try { relLogger.Warn($"[Orchestrator] Metadata refresh failed: {ex.Message}"); } catch { }
            
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
            try { relLogger.Info("[Orchestrator] Final result: SUCCESS"); } catch { }
            
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
            try { relLogger.Warn($"[Orchestrator] Final result: FAILURE ({failureResult}) — {metadataError}"); } catch { }
            
            // Update state to Finished
            downloadHistory.UpdateState(artistId, releaseFolderName, DownloadState.Finished, "Download process failed during metadata validation");
            
            logger.LogWarning("[StartDownload] Download failed for {ArtistId}/{ReleaseFolder} after metadata validation: {Error}", artistId, releaseFolderName, metadataError);
        }

        // Log Phase 2 completion time
        var phase2Duration = DateTime.UtcNow - phase2StartTime;
        logger.LogInformation("[StartDownload] Phase 2 completed in {Duration:g} for {ArtistId}/{ReleaseFolder}", phase2Duration, artistId, releaseFolderName);
        try { relLogger.Info($"[Orchestrator] Phase 2 completed in {phase2Duration:g}"); } catch { }

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
