using Microsoft.Extensions.Options;
using MusicGQL.Features.Downloads.Services;
using MusicGQL.Features.External.Downloads.Prowlarr.Configuration;
using MusicGQL.Features.External.Downloads.QBittorrent;
using MusicGQL.Features.External.Downloads.Sabnzbd;
using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.External.Downloads.Prowlarr;

public class ProwlarrDownloadProvider(
    IProwlarrClient prowlarr,
    QBittorrentClient qb,
    SabnzbdClient sab,
    IOptions<ProwlarrOptions> prowlarrOptions,
    IOptions<MusicGQL.Features.External.Downloads.Sabnzbd.Configuration.SabnzbdOptions> sabOptions,
    ServerSettings.ServerSettingsAccessor serverSettingsAccessor,
    ILogger<ProwlarrDownloadProvider> logger,
    DownloadLogPathProvider logPathProvider,
    ServerLibraryCache cache
) : IDownloadProvider
{
    private DownloadLogger? serviceLogger;

    public async Task<DownloadLogger> GetLogger()
    {
        if (serviceLogger == null)
        {
            var path = await logPathProvider.GetServiceLogFilePathAsync("prowlarr");
            serviceLogger = new DownloadLogger(path);
        }
        return serviceLogger;
    }

    public async Task<bool> TryDownloadReleaseAsync(
        string artistId,
        string releaseFolderName,
        string artistName,
        string releaseTitle,
        string targetDirectory,
        IReadOnlyList<int> allowedOfficialCounts,
        IReadOnlyList<int> allowedOfficialDigitalCounts,
        CancellationToken cancellationToken
    )
    {
        var serviceLogger = await GetLogger();
        // Initialize per-release log - CRASH if this fails as requested
        logger.LogInformation("[Prowlarr] ===== STARTING TryDownloadReleaseAsync =====");
        serviceLogger.Info("[Prowlarr] ===== STARTING TryDownloadReleaseAsync =====");
        logger.LogInformation(
            "[Prowlarr] Parameters - ArtistId: {ArtistId}, ReleaseFolderName: {ReleaseFolderName}",
            artistId,
            releaseFolderName
        );
        serviceLogger.Info(
            $"[Prowlarr] Parameters - ArtistId: {artistId}, ReleaseFolderName: {releaseFolderName}"
        );
        logger.LogInformation(
            "[Prowlarr] Parameters - ArtistName: {ArtistName}, ReleaseTitle: {ReleaseTitle}",
            artistName,
            releaseTitle
        );
        serviceLogger.Info(
            $"[Prowlarr] Parameters - ArtistName: {artistName}, ReleaseTitle: {releaseTitle}"
        );
        logger.LogInformation(
            "[Prowlarr] Parameters - TargetDirectory: {TargetDirectory}",
            targetDirectory
        );
        serviceLogger.Info($"[Prowlarr] Parameters - TargetDirectory: {targetDirectory}");
        logger.LogInformation(
            "[Prowlarr] Parameters - AllowedOfficialCounts: {Counts}",
            string.Join(", ", allowedOfficialCounts)
        );
        serviceLogger.Info(
            $"[Prowlarr] Parameters - AllowedOfficialCounts: {string.Join(", ", allowedOfficialCounts)}"
        );
        logger.LogInformation(
            "[Prowlarr] Parameters - AllowedOfficialDigitalCounts: {Counts}",
            string.Join(", ", allowedOfficialDigitalCounts)
        );
        serviceLogger.Info(
            $"[Prowlarr] Parameters - AllowedOfficialDigitalCounts: {string.Join(", ", allowedOfficialDigitalCounts)}"
        );

        logger.LogInformation("[Prowlarr] Initializing per-release logger...");
        serviceLogger.Info("[Prowlarr] Initializing per-release logger...");
        string? logPath = null;
        try
        {
            logPath = await logPathProvider.GetReleaseLogFilePathAsync(
                artistName,
                releaseTitle,
                cancellationToken
            );
            logger.LogInformation(
                "[Prowlarr] Log path obtained: {LogPath}",
                logPath ?? "null/empty"
            );
            serviceLogger.Info($"[Prowlarr] Log path obtained: {logPath ?? "null/empty"}");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[Prowlarr] CRITICAL ERROR: Failed to get release log path - CRASHING APPLICATION"
            );
            serviceLogger.Error(
                "[Prowlarr] CRITICAL ERROR: Failed to get release log path - CRASHING APPLICATION"
            );
            Environment.Exit(-1);
            return false; // This line will never be reached
        }

        DownloadLogger? relLoggerImpl = null;
        IDownloadLogger relLogger = new NullDownloadLogger();

        if (!string.IsNullOrWhiteSpace(logPath))
        {
            logger.LogInformation(
                "[Prowlarr] Creating DownloadLogger with path: {LogPath}",
                logPath
            );
            serviceLogger.Info($"[Prowlarr] Creating DownloadLogger with path: {logPath}");
            try
            {
                relLoggerImpl = new DownloadLogger(logPath!);
                relLogger = relLoggerImpl;
                logger.LogInformation("[Prowlarr] DownloadLogger created successfully");
                serviceLogger.Info("[Prowlarr] DownloadLogger created successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "[Prowlarr] CRITICAL ERROR: Failed to create DownloadLogger - CRASHING APPLICATION"
                );
                serviceLogger.Error(
                    "[Prowlarr] CRITICAL ERROR: Failed to create DownloadLogger - CRASHING APPLICATION"
                );
                Environment.Exit(-1);
                return false; // This line will never be reached
            }
        }
        else
        {
            logger.LogWarning("[Prowlarr] Log path is null or empty, using NullDownloadLogger");
            serviceLogger.Warn("[Prowlarr] Log path is null or empty, using NullDownloadLogger");
        }

        logger.LogInformation("[Prowlarr] Logger initialization complete");
        serviceLogger.Info("[Prowlarr] Logger initialization complete");

        // Now we can use relLogger safely
        relLogger.Info("[Prowlarr] ===== STARTING TryDownloadReleaseAsync =====");
        relLogger.Info(
            $"[Prowlarr] Parameters - ArtistId: {artistId}, ReleaseFolderName: {releaseFolderName}"
        );
        relLogger.Info(
            $"[Prowlarr] Parameters - ArtistName: {artistName}, ReleaseTitle: {releaseTitle}"
        );
        relLogger.Info($"[Prowlarr] Parameters - TargetDirectory: {targetDirectory}");
        relLogger.Info(
            $"[Prowlarr] Parameters - AllowedOfficialCounts: {string.Join(", ", allowedOfficialCounts)}"
        );
        relLogger.Info(
            $"[Prowlarr] Parameters - AllowedOfficialDigitalCounts: {string.Join(", ", allowedOfficialDigitalCounts)}"
        );

        try
        {
            // Read toggles
            logger.LogInformation("[Prowlarr] Reading server settings...");
            var settings = await serverSettingsAccessor.GetAsync();
            logger.LogInformation("[Prowlarr] Server settings loaded successfully");

            var allowSab = settings.EnableSabnzbdDownloader;
            var allowQbit = settings.EnableQBittorrentDownloader;
            logger.LogInformation(
                "[Prowlarr] Downloader settings - SABnzbd: {SAB}, qBittorrent: {QB}",
                allowSab,
                allowQbit
            );

            // 1) Search Prowlarr for nzb/magnet results
            logger.LogInformation("[Prowlarr] ===== PHASE 1: Search Prowlarr for results =====");
            logger.LogInformation(
                "[Prowlarr] Begin provider for {Artist} - {Release}",
                artistName,
                releaseTitle
            );
            relLogger.Info($"[Prowlarr] Begin provider for {artistName} - {releaseTitle}");

            // Get year information from cache for better search specificity
            logger.LogInformation("[Prowlarr] Getting year information from cache...");
            var cachedRelease = await cache.GetReleaseByArtistAndFolderAsync(
                artistId,
                releaseFolderName
            );
            logger.LogInformation(
                "[Prowlarr] Cache lookup result: {HasResult}",
                cachedRelease != null
            );

            int? year = null;
            if (cachedRelease?.JsonRelease?.FirstReleaseYear != null)
            {
                logger.LogInformation(
                    "[Prowlarr] FirstReleaseYear found: {Year}",
                    cachedRelease.JsonRelease.FirstReleaseYear
                );
                if (int.TryParse(cachedRelease.JsonRelease.FirstReleaseYear, out int parsedYear))
                {
                    year = parsedYear;
                    logger.LogInformation("[Prowlarr] Successfully parsed year: {Year}", year);
                }
                else
                {
                    logger.LogWarning(
                        "[Prowlarr] Failed to parse year from: {YearString}",
                        cachedRelease.JsonRelease.FirstReleaseYear
                    );
                }
            }
            else
            {
                logger.LogInformation("[Prowlarr] No year information found in cache");
            }

            // Validate configuration before searching, to avoid silent fast-fail
            logger.LogInformation("[Prowlarr] Validating Prowlarr configuration...");
            var baseUrl = prowlarrOptions.Value.BaseUrl;
            var apiKey = prowlarrOptions.Value.ApiKey;

            logger.LogInformation(
                "[Prowlarr] Configuration check - BaseUrl: {BaseUrl}, ApiKey: {HasApiKey}",
                baseUrl ?? "null",
                !string.IsNullOrWhiteSpace(apiKey)
            );

            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
            {
                logger.LogError(
                    "[Prowlarr] CRITICAL: Configuration validation FAILED - RETURNING FALSE"
                );
                relLogger.Warn(
                    $"[Prowlarr] Configuration validation FAILED - BaseUrl='{baseUrl ?? ""}', ApiKeyConfigured={!string.IsNullOrWhiteSpace(apiKey)}"
                );
                return false;
            }

            logger.LogInformation("[Prowlarr] Configuration validation PASSED");

            // Execute the search
            logger.LogInformation("[Prowlarr] Executing Prowlarr search...");
            logger.LogInformation(
                "[Prowlarr] Search parameters - ArtistName: {Artist}, ReleaseTitle: {Release}, Year: {Year}",
                artistName,
                releaseTitle,
                year?.ToString() ?? "null"
            );

            var results = await prowlarr.SearchAlbumAsync(
                artistName,
                releaseTitle,
                year,
                cancellationToken,
                relLogger
            );
            logger.LogInformation(
                "[Prowlarr] Search completed - Results count: {Count}",
                results.Count
            );

            if (results.Count > 0)
            {
                logger.LogInformation(
                    "[Prowlarr] Search returned results, proceeding to filtering..."
                );
            }
            else
            {
                logger.LogWarning("[Prowlarr] Search returned NO RESULTS - RETURNING FALSE");
            }

            relLogger.Info($"[Prowlarr] Search returned {results.Count} results");

            logger.LogInformation("[Prowlarr] ===== PHASE 2: Process and filter results =====");
            logger.LogInformation("[Prowlarr] Processing {Count} raw results...", results.Count);

            // Log first few raw results
            int rawCount = 0;
            foreach (var r in results)
            {
                if (rawCount++ >= 10)
                {
                    relLogger.Info("[Prowlarr] (more results omitted)…");
                    logger.LogInformation(
                        "[Prowlarr] Showing first 10 results, {Remaining} more omitted",
                        Math.Max(0, results.Count - 10)
                    );
                    break;
                }

                logger.LogInformation(
                    "[Prowlarr] Raw result {Index}: title='{Title}', size={Size}, magnet={HasMagnet}, url={HasUrl}, indexer={IndexerId}",
                    rawCount,
                    r.Title,
                    r.Size,
                    !string.IsNullOrWhiteSpace(r.MagnetUrl),
                    !string.IsNullOrWhiteSpace(r.DownloadUrl),
                    r.IndexerId
                );
                relLogger.Info(
                    $"[Prowlarr] Raw result: title={r.Title} size={r.Size} magnet={(!string.IsNullOrWhiteSpace(r.MagnetUrl) ? "yes" : "no")} url={(!string.IsNullOrWhiteSpace(r.DownloadUrl) ? "yes" : "no")} idx={r.IndexerId}"
                );
            }

            logger.LogInformation("[Prowlarr] Starting result filtering and scoring...");

            // Prefer items that look like full releases and larger sizes first
            // Filter and score results for music albums only; reject obvious mismatches (e.g., TV packs)
            var preFilter = results;
            logger.LogInformation("[Prowlarr] Pre-filter count: {Count}", preFilter.Count);

            var filtered = preFilter
                .Where(r => IsLikelyMusicAlbum(r, artistName, releaseTitle))
                .OrderByDescending(r => Score(r, artistName, releaseTitle))
                .ThenByDescending(r => r.Size ?? 0)
                .ToList();

            logger.LogInformation("[Prowlarr] Post-filter count: {Count}", filtered.Count);
            relLogger.Info(
                $"[Prowlarr] Filtered results: before={preFilter.Count} after={filtered.Count}"
            );

            // Log filtering results with detailed rejection reasons
            if (filtered.Count == 0 && preFilter.Count > 0)
            {
                logger.LogError(
                    "[Prowlarr] CRITICAL: ALL results were filtered out - investigating rejection reasons..."
                );
                int rejectionCount = 0;
                foreach (var r in preFilter)
                {
                    if (rejectionCount++ >= 10)
                    {
                        relLogger.Info("[Prowlarr] (more rejected omitted)…");
                        logger.LogWarning(
                            "[Prowlarr] Showing first 10 rejections, {Remaining} more omitted",
                            Math.Max(0, preFilter.Count - 10)
                        );
                        break;
                    }

                    var reasons = GetProviderRejectionReasons(r, artistName, releaseTitle);
                    logger.LogError(
                        "[Prowlarr] REJECTED result {Index}: '{Title}' - Reasons: [{Reasons}]",
                        rejectionCount,
                        r.Title,
                        string.Join(", ", reasons)
                    );
                    relLogger.Info(
                        $"[Prowlarr] Rejected: title={r.Title} reasons=[{string.Join(',', reasons)}]"
                    );
                }
            }
            else if (filtered.Count > 0)
            {
                logger.LogInformation(
                    "[Prowlarr] Some results passed filtering - showing top results..."
                );
                int passedCount = 0;
                foreach (var r in filtered)
                {
                    if (passedCount++ >= 5)
                    {
                        logger.LogInformation("[Prowlarr] (showing first 5 passed results)...");
                        break;
                    }

                    var score = Score(r, artistName, releaseTitle);
                    logger.LogInformation(
                        "[Prowlarr] PASSED result {Index}: '{Title}' - Score: {Score}, Size: {Size}",
                        passedCount,
                        r.Title,
                        score,
                        r.Size
                    );
                }
            }

            results = filtered;
            if (results.Count == 0)
            {
                logger.LogError(
                    "[Prowlarr] CRITICAL FAILURE: No results after filtering - RETURNING FALSE"
                );
                relLogger.Warn("[Prowlarr] No results after filtering");
                return false;
            }

            logger.LogInformation(
                "[Prowlarr] Results ready for download processing - count: {Count}",
                results.Count
            );

            // 2) Prefer NZB if available; otherwise fallback to magnet
            // Prefer Prowlarr-provided downloadUrl; many setups do not include .nzb extension
            // Extra guard: only consider results whose title clearly matches artist and release
            bool TitleMatches(string? title)
            {
                if (string.IsNullOrWhiteSpace(title))
                    return false;
                var t = title.ToLowerInvariant();
                var a = (artistName ?? string.Empty).ToLowerInvariant();
                var rel = (releaseTitle ?? string.Empty).ToLowerInvariant();
                // Require both artist and at least half of album words
                if (!t.Contains(a))
                    return false;
                var albumWords = rel.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var titleWords = t.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var matchCount = albumWords.Count(w =>
                    titleWords.Any(x => x.Contains(w) || w.Contains(x))
                );
                return matchCount >= Math.Max(1, albumWords.Length / 2);
            }

            // Prefer NZB for SAB, but explicitly avoid .torrent URLs here
            static bool LooksLikeDiscography(string? title)
            {
                if (string.IsNullOrWhiteSpace(title))
                    return false;
                var t = title.ToLowerInvariant();
                string[] bad =
                [
                    "discography",
                    "collection",
                    "anthology",
                    "complete",
                    "box set",
                    "boxset",
                    "complete works",
                    "singles collection",
                    "mega pack",
                ];
                return bad.Any(k => t.Contains(k));
            }

            var selection = ProwlarrSelectionLogic.Decide(
                results,
                artistName,
                releaseTitle,
                allowSab,
                allowQbit,
                settings.DiscographyEnabled
            );

            // Helper: attempt torrent/magnet fallback when NZB path fails
            async Task<bool> TryTorrentFallbackAsync()
            {
                if (!allowQbit)
                {
                    logger.LogInformation(
                        "[Prowlarr] Torrent fallback skipped (qBittorrent disabled)"
                    );
                    return false;
                }

                // Prefer magnet first
                var magnetCand = results.FirstOrDefault(r =>
                    !string.IsNullOrWhiteSpace(r.MagnetUrl)
                    && TitleMatchesLocal(r.Title, artistName, releaseTitle)
                    && !LooksLikeDiscography(r.Title)
                );
                if (magnetCand is not null)
                {
                    logger.LogInformation(
                        "[Prowlarr] Torrent fallback: handing off magnet to qBittorrent: {Title}",
                        magnetCand.Title ?? "(no title)"
                    );
                    try
                    {
                        relLogger.Info(
                            $"[Prowlarr] Torrent fallback: handing off magnet to qBittorrent: {magnetCand.Title}"
                        );
                    }
                    catch { }
                    var okMag = await qb.AddMagnetAsync(
                        magnetCand.MagnetUrl!,
                        null,
                        cancellationToken
                    );
                    if (okMag)
                    {
                        try
                        {
                            relLogger.Info("[Prowlarr] qBittorrent accepted magnet (fallback)");
                        }
                        catch { }
                        return true;
                    }
                    logger.LogWarning("[Prowlarr] qBittorrent did not accept magnet (fallback)");
                    try
                    {
                        relLogger.Warn("[Prowlarr] qBittorrent did not accept magnet (fallback)");
                    }
                    catch { }
                }

                var torrentCand = results.FirstOrDefault(r =>
                    !string.IsNullOrWhiteSpace(r.DownloadUrl)
                    && LooksLikeTorrentUrl(r.DownloadUrl!)
                    && TitleMatchesLocal(r.Title, artistName, releaseTitle)
                    && !LooksLikeDiscography(r.Title)
                );
                if (torrentCand is not null)
                {
                    var tUrl = EnsureProwlarrApiKey(torrentCand.DownloadUrl!);
                    logger.LogInformation(
                        "[Prowlarr] Torrent fallback: handing off .torrent URL to qBittorrent: {Title}",
                        torrentCand.Title ?? "(no title)"
                    );
                    try
                    {
                        relLogger.Info(
                            $"[Prowlarr] Torrent fallback: handing off .torrent URL to qBittorrent: {torrentCand.Title}"
                        );
                    }
                    catch { }
                    var okT = await qb.AddByUrlAsync(tUrl, null, cancellationToken);
                    if (okT)
                    {
                        try
                        {
                            relLogger.Info(
                                "[Prowlarr] qBittorrent accepted .torrent URL (fallback)"
                            );
                        }
                        catch { }
                        return true;
                    }
                }

                return false;
            }

            if (
                selection.Type == ProwlarrSelectionType.Nzb
                && selection.Release is not null
                && !string.IsNullOrWhiteSpace(selection.UrlOrMagnet)
            )
            {
                var nzb = selection.Release;
                var url = EnsureProwlarrApiKey(selection.UrlOrMagnet!);
                logger.LogInformation(
                    "[Prowlarr] Uploading NZB to SABnzbd: {Title}",
                    nzb.Title ?? "(no title)"
                );
                try
                {
                    try
                    {
                        relLogger.Info($"[Prowlarr] Uploading NZB to SABnzbd: {nzb.Title}");
                    }
                    catch { }

                    using var http = new HttpClient();
                    var bytes = await http.GetByteArrayAsync(url, cancellationToken);
                    // Validate the payload looks like an NZB, not a torrent
                    if (!LooksLikeNzb(bytes))
                    {
                        logger.LogWarning(
                            "[Prowlarr] Fetched content does not look like NZB, skipping upload (likely torrent)"
                        );
                        try
                        {
                            relLogger.Warn("[Prowlarr] Content not NZB; skipping");
                        }
                        catch { }

                        // Try torrent fallback now
                        var okFallback = await TryTorrentFallbackAsync();
                        if (okFallback)
                            return true;
                    }
                    else
                    {
                        var fileName = (nzb.Title ?? "download").Replace(' ', '+') + ".nzb";
                        var okUpload = await sab.AddNzbByContentAsync(
                            bytes,
                            fileName,
                            cancellationToken,
                            pathOverride: $"mmusic/{artistId}/{releaseFolderName}",
                            nzbName: $"{artistName} - {releaseTitle}"
                        );
                        if (okUpload)
                        {
                            try
                            {
                                relLogger.Info("[Prowlarr] SABnzbd accepted NZB upload");
                            }
                            catch { }

                            return true;
                        }

                        logger.LogWarning("[Prowlarr] SABnzbd rejected NZB content upload");
                        try
                        {
                            relLogger.Warn("[Prowlarr] SABnzbd rejected NZB content upload");
                        }
                        catch { }

                        // Try torrent fallback after rejection
                        var okFallback2 = await TryTorrentFallbackAsync();
                        if (okFallback2)
                            return true;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "[Prowlarr] Failed fetching/uploading NZB content");
                    try
                    {
                        relLogger.Error($"[Prowlarr] Exception during NZB upload: {ex.Message}");
                    }
                    catch { }

                    // Try torrent fallback on exception
                    var okFallback3 = await TryTorrentFallbackAsync();
                    if (okFallback3)
                        return true;
                }
            }

            // If initial selection is magnet/torrent, attempt them in order
            if (
                selection.Type == ProwlarrSelectionType.Magnet
                && selection.UrlOrMagnet is not null
                && selection.Release is not null
            )
            {
                var magnet = selection.Release;
                logger.LogInformation(
                    "[Prowlarr] Handing off magnet to qBittorrent: {Title}",
                    magnet.Title ?? "(no title)"
                );
                try
                {
                    relLogger.Info($"[Prowlarr] Handing off magnet to qBittorrent: {magnet.Title}");
                }
                catch { }

                var ok = await qb.AddMagnetAsync(selection.UrlOrMagnet!, null, cancellationToken);
                if (ok)
                {
                    try
                    {
                        relLogger.Info("[Prowlarr] qBittorrent accepted magnet");
                    }
                    catch { }

                    return true;
                }

                logger.LogWarning("[Prowlarr] qBittorrent did not accept magnet handoff");
                try
                {
                    relLogger.Warn("[Prowlarr] qBittorrent did not accept magnet");
                }
                catch { }
            }

            // Fallbacks by type: if we have a torrent URL, send to qBittorrent; if generic HTTP and NZB-like, send to SAB
            if (
                selection.Type == ProwlarrSelectionType.Torrent
                && !string.IsNullOrWhiteSpace(selection.UrlOrMagnet)
            )
            {
                logger.LogInformation("[Prowlarr] Handing off .torrent URL to qBittorrent");
                try
                {
                    relLogger.Info("[Prowlarr] Handing off .torrent URL to qBittorrent");
                }
                catch { }

                var okT = await qb.AddByUrlAsync(
                    EnsureProwlarrApiKey(selection.UrlOrMagnet!),
                    null,
                    cancellationToken
                );
                if (okT)
                {
                    try
                    {
                        relLogger.Info("[Prowlarr] qBittorrent accepted .torrent URL");
                    }
                    catch { }

                    return true;
                }
            }

            var httpUrl =
                selection.Type == ProwlarrSelectionType.Nzb ? selection.UrlOrMagnet : null;

            // Discography path (transparent): if discography-like and allowed, handoff to SAB/qBit into staging
            var anyDiscography = results.FirstOrDefault(r => LooksLikeDiscography(r.Title));
            if (anyDiscography != null && settings.DiscographyEnabled)
            {
                var staging = settings.DiscographyStagingPath;
                if (!string.IsNullOrWhiteSpace(staging))
                {
                    // Prefer NZB otherwise magnet/torrent URL
                    var discNzb = results.FirstOrDefault(r =>
                        LooksLikeDiscography(r.Title)
                        && !string.IsNullOrWhiteSpace(r.DownloadUrl)
                        && !LooksLikeTorrentUrl(r.DownloadUrl!)
                    );
                    if (allowSab && discNzb is not null)
                    {
                        try
                        {
                            var url = EnsureProwlarrApiKey(discNzb.DownloadUrl!);
                            using var http = new HttpClient();
                            var bytes = await http.GetByteArrayAsync(url, cancellationToken);
                            if (LooksLikeNzb(bytes))
                            {
                                var ok = await sab.AddNzbByContentAsync(
                                    bytes,
                                    (discNzb.Title ?? "discography").Replace(' ', '+') + ".nzb",
                                    cancellationToken,
                                    pathOverride: $"discography/{artistId}",
                                    nzbName: $"{artistName} - DISCography bundle"
                                );
                                if (ok)
                                {
                                    try
                                    {
                                        relLogger.Info(
                                            "[Prowlarr] Discography NZB handed to SAB for staging"
                                        );
                                    }
                                    catch { }
                                }
                            }
                        }
                        catch { }
                    }
                    else if (allowQbit)
                    {
                        var discTorrent =
                            results
                                .FirstOrDefault(r =>
                                    LooksLikeDiscography(r.Title)
                                    && !string.IsNullOrWhiteSpace(r.DownloadUrl)
                                    && LooksLikeTorrentUrl(r.DownloadUrl!)
                                )
                                ?.DownloadUrl
                            ?? results
                                .FirstOrDefault(r =>
                                    LooksLikeDiscography(r.Title)
                                    && !string.IsNullOrWhiteSpace(r.MagnetUrl)
                                )
                                ?.MagnetUrl;
                        if (!string.IsNullOrWhiteSpace(discTorrent))
                        {
                            try
                            {
                                relLogger.Info(
                                    "[Prowlarr] Discography torrent/magnet handed to qBittorrent for staging"
                                );
                            }
                            catch { }

                            await qb.AddByUrlAsync(
                                EnsureProwlarrApiKey(discTorrent!),
                                null,
                                cancellationToken
                            );
                        }
                    }
                }
            }

            if (
                allowSab
                && !string.IsNullOrWhiteSpace(httpUrl)
                && (httpUrl!.StartsWith("http://") || httpUrl.StartsWith("https://"))
            )
            {
                logger.LogInformation(
                    "[Prowlarr] Final HTTP URL found - attempting SABnzbd download"
                );
                var url2 = EnsureProwlarrApiKey(httpUrl);
                logger.LogInformation(
                    "[Prowlarr] Final HTTP URL (pre-rewrite): {Url}",
                    SanitizeUrlForLogs(url2)
                );
                relLogger.Info(
                    $"[Prowlarr] Attempting SABnzbd handoff using HTTP URL (pre-rewrite): {SanitizeUrlForLogs(url2)}"
                );

                // If SAB is in Docker and needs to fetch from Prowlarr, rewrite the base to SAB's internal view of Prowlarr
                var sabInternalProwlarr = sabOptions.Value.BaseUrlToProwlarr;
                var prowlarrExternal = prowlarrOptions.Value.BaseUrl;
                var rewriteUrl =
                    MusicGQL.Features.External.Downloads.InterServiceUrlRewriter.RewriteBase(
                        url2,
                        prowlarrExternal,
                        sabInternalProwlarr
                    );

                logger.LogInformation(
                    "[Prowlarr] SAB handoff rewrite base from {From} to {To}",
                    prowlarrExternal ?? "(null)",
                    sabInternalProwlarr ?? "(null)"
                );
                logger.LogInformation(
                    "[Prowlarr] SAB handoff URL (post-rewrite): {Url}",
                    SanitizeUrlForLogs(rewriteUrl)
                );
                relLogger.Info(
                    $"[Prowlarr] SAB handoff URL (post-rewrite): {SanitizeUrlForLogs(rewriteUrl)}"
                );

                var ok2 = await sab.AddNzbByUrlAsync(
                    rewriteUrl,
                    $"{artistName} - {releaseTitle}",
                    cancellationToken
                );
                logger.LogInformation("[Prowlarr] Final SABnzbd HTTP result: {Success}", ok2);

                if (ok2)
                {
                    logger.LogInformation(
                        "[Prowlarr] SUCCESS: Final HTTP URL handed off to SABnzbd successfully"
                    );
                    relLogger.Info("[Prowlarr] SABnzbd accepted HTTP handoff");
                    return true;
                }
                else
                {
                    logger.LogWarning("[Prowlarr] SABnzbd rejected final HTTP handoff");
                }
            }

            // Diagnostic: log a few candidates and reasons
            logger.LogInformation("[Prowlarr] ===== FINAL DIAGNOSTIC ANALYSIS =====");
            logger.LogInformation(
                "[Prowlarr] Analyzing first 3 candidates for rejection reasons..."
            );

            try
            {
                int candidateCount = 0;
                foreach (var r in results.Take(3))
                {
                    candidateCount++;
                    var reasons = new List<string>();
                    if (!TitleMatches(r.Title))
                        reasons.Add("titleMismatch");
                    if (LooksLikeDiscography(r.Title))
                        reasons.Add("discography");
                    if (
                        string.IsNullOrWhiteSpace(r.DownloadUrl)
                        && string.IsNullOrWhiteSpace(r.MagnetUrl)
                    )
                        reasons.Add("noLink");

                    logger.LogInformation(
                        "[Prowlarr] Candidate {Index}: title='{Title}' size={Size} reasons=[{Reasons}]",
                        candidateCount,
                        r.Title ?? "",
                        r.Size,
                        string.Join(',', reasons)
                    );
                    relLogger.Info(
                        $"[Prowlarr] Candidate: title={r.Title} size={r.Size} reasons=[{string.Join(',', reasons)}]"
                    );
                }

                logger.LogInformation(
                    "[Prowlarr] Diagnostic analysis complete - {Count} candidates examined",
                    candidateCount
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[Prowlarr] ERROR during diagnostic analysis");
            }

            logger.LogError("[Prowlarr] CRITICAL FAILURE: All attempts failed - RETURNING FALSE");
            logger.LogInformation(
                "Prowlarr provider found results but failed to hand off to downloader (no acceptable candidates)"
            );
            relLogger.Warn(
                "[Prowlarr] No acceptable candidates after filtering. First few candidates were logged above with reasons."
            );

            return false;
        }
        finally
        {
            logger.LogInformation("[Prowlarr] ===== ENDING TryDownloadReleaseAsync =====");
            logger.LogInformation("[Prowlarr] Disposing logger resources...");
            relLoggerImpl?.Dispose();
            logger.LogInformation("[Prowlarr] Logger resources disposed");
        }
    }

    private static IEnumerable<string> GetProviderRejectionReasons(
        ProwlarrRelease r,
        string artistName,
        string releaseTitle
    )
    {
        var reasons = new List<string>();
        try
        {
            bool titleOk = TitleMatchesLocal(r.Title, artistName, releaseTitle);
            if (!titleOk)
                reasons.Add("titleMismatch");
            if (LooksLikeDiscography(r.Title))
                reasons.Add("discography");
            if (string.IsNullOrWhiteSpace(r.DownloadUrl) && string.IsNullOrWhiteSpace(r.MagnetUrl))
                reasons.Add("noLink");
        }
        catch { }

        return reasons;
    }

    private static bool TitleMatchesLocal(string? title, string artistName, string releaseTitle)
    {
        if (string.IsNullOrWhiteSpace(title))
            return false;
        var t = title.ToLowerInvariant();
        var a = (artistName ?? string.Empty).ToLowerInvariant();
        var rel = (releaseTitle ?? string.Empty).ToLowerInvariant();
        if (!t.Contains(a))
            return false;
        var albumWords = rel.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleWords = t.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var matchCount = albumWords.Count(w => titleWords.Any(x => x.Contains(w) || w.Contains(x)));
        return matchCount >= Math.Max(1, albumWords.Length / 2);
    }

    private string EnsureProwlarrApiKey(string downloadUrl)
    {
        try
        {
            var baseUrl = prowlarrOptions.Value.BaseUrl?.TrimEnd('/');
            var apiKey = prowlarrOptions.Value.ApiKey;
            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
                return downloadUrl;

            // If the download url points to prowlarr and lacks apikey, append it
            if (downloadUrl.StartsWith("/"))
            {
                downloadUrl = baseUrl + downloadUrl;
            }

            if (
                downloadUrl.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase)
                && downloadUrl.IndexOf("apikey=", StringComparison.OrdinalIgnoreCase) < 0
            )
            {
                var sep = downloadUrl.Contains('?') ? '&' : '?';
                return downloadUrl + sep + "apikey=" + Uri.EscapeDataString(apiKey);
            }

            return downloadUrl;
        }
        catch
        {
            return downloadUrl;
        }
    }

    private static bool LooksLikeNzb(byte[] bytes)
    {
        try
        {
            // Check for ASCII XML signature and <nzb> tag in the first few KB
            var sampleLen = Math.Min(bytes.Length, 4096);
            var sample = System.Text.Encoding.UTF8.GetString(bytes, 0, sampleLen);
            if (sample.IndexOf("<nzb", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            // Common torrent indicator near start
            if (sample.IndexOf("announce", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;
            // Fallback: looks like XML
            if (sample.TrimStart().StartsWith("<?xml"))
                return true;
        }
        catch { }

        return false;
    }

    private static bool LooksLikeTorrentUrl(string downloadUrl)
    {
        try
        {
            return downloadUrl.EndsWith(".torrent", StringComparison.OrdinalIgnoreCase)
                || downloadUrl.Contains("torrent", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static bool LooksLikeDiscography(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return false;
        var t = title.ToLowerInvariant();
        string[] bad =
        [
            "discography",
            "collection",
            "anthology",
            "complete",
            "box set",
            "boxset",
            "complete works",
            "singles collection",
            "mega pack",
        ];
        return bad.Any(k => t.Contains(k));
    }

    private static string SanitizeUrlForLogs(string url)
    {
        try
        {
            // Mask common API key query parameters while preserving the base/host
            return System.Text.RegularExpressions.Regex.Replace(
                url,
                "(?i)([?&](?:apikey|api_key|apiKey)=)[^&#]+",
                "$1***"
            );
        }
        catch
        {
            return url;
        }
    }

    private static bool IsLikelyMusicAlbum(
        ProwlarrRelease r,
        string artistName,
        string releaseTitle
    )
    {
        var title = (r.Title ?? string.Empty).ToLowerInvariant();
        // Reject obvious non-music signals
        if (
            title.Contains("s01")
            || title.Contains("s1 ")
            || title.Contains(" s11 ")
            || title.Contains("season ")
            || title.Contains("s11-s")
            || title.Contains("e01")
            || title.Contains("1080p")
            || title.Contains("2160p")
        )
        {
            // Allow resolution presence for music only if artist and release tokens both match strongly
            if (Score(r, artistName, releaseTitle) < 5)
                return false;
        }

        if (
            title.Contains("simpsons")
            || title.Contains("spongebob")
            || title.Contains("season")
            || title.Contains("episode")
        )
            return false;
        if (
            title.Contains("x264")
            || title.Contains("x265")
            || title.Contains("h264")
            || title.Contains("h265")
        )
        {
            if (Score(r, artistName, releaseTitle) < 6)
                return false;
        }

        // Prefer music-specific tags
        if (
            title.Contains("flac")
            || title.Contains("mp3")
            || title.Contains("lossless")
            || title.Contains("album")
            || title.Contains("disc ")
        )
        {
            return true;
        }

        // Fallback: require both artist and release tokens
        return Score(r, artistName, releaseTitle) >= 4;
    }

    private static int Score(ProwlarrRelease r, string artistName, string releaseTitle)
    {
        static string Norm(string s)
        {
            s = s.Replace("’", "'").ToLowerInvariant();
            var keep = new System.Text.StringBuilder(s.Length);
            foreach (var ch in s)
            {
                if (char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch))
                    keep.Append(ch);
            }

            return System
                .Text.RegularExpressions.Regex.Replace(keep.ToString(), "\\s+", " ")
                .Trim();
        }

        var title = Norm(r.Title ?? string.Empty);
        var artist = Norm(artistName);
        var rel = Norm(releaseTitle);
        int score = 0;
        if (!string.IsNullOrWhiteSpace(artist) && title.Contains(artist))
            score += Math.Min(artist.Length, 5);
        if (!string.IsNullOrWhiteSpace(rel) && title.Contains(rel))
            score += Math.Min(rel.Length, 5);
        // Light bonus for music tags
        if (title.Contains("flac"))
            score += 2;
        if (title.Contains("mp3"))
            score += 1;
        if (title.Contains("lossless"))
            score += 2;
        if (title.Contains("album"))
            score += 1;
        return score;
    }
}
