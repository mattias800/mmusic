using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MusicGQL.Features.Downloads.Services;
using MusicGQL.Features.External.Downloads.Sabnzbd.Configuration;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Writer;

namespace MusicGQL.Features.External.Downloads.Sabnzbd;

public class SabnzbdFinalizeService(
    IOptions<SabnzbdOptions> options,
    ILogger<SabnzbdFinalizeService> logger,
    ServerLibraryCache cache,
    ServerSettings.ServerSettingsAccessor serverSettingsAccessor,
    IServiceScopeFactory scopeFactory,
    SabnzbdClient sabnzbdClient,
    DownloadLogPathProvider logPathProvider
)
{
    private MusicGQL.Features.Downloads.Services.DownloadLogger? serviceLogger;
    private static readonly string[] AudioExtensions = new[]
    {
        ".mp3",
        ".flac",
        ".m4a",
        ".wav",
        ".ogg",
    };

    public async Task<MusicGQL.Features.Downloads.Services.DownloadLogger> GetLogger()
    {
        if (serviceLogger == null)
        {
            var path = await logPathProvider.GetServiceLogFilePathAsync("sabnzbd");
            serviceLogger = new MusicGQL.Features.Downloads.Services.DownloadLogger(path);
        }
        return serviceLogger;
    }

    // Helper: extract disc and track numbers from a file name
    private static (int disc, int track) ExtractDiscTrackFromName(string? name)
    {
        int disc = 1;
        int track = -1;
        try
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var lower = name!.ToLowerInvariant();
                var m = System.Text.RegularExpressions.Regex.Match(
                    lower,
                    "\\b(?:cd|disc|disk|digital\\s*media)\\s*(\\d+)\\b",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                if (m.Success && int.TryParse(m.Groups[1].Value, out var d))
                    disc = d;
                var m2 = System.Text.RegularExpressions.Regex.Match(
                    name,
                    "-\\s*(\\d{1,3})\\s*-",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                if (m2.Success && int.TryParse(m2.Groups[1].Value, out var t2))
                    track = t2;
                if (track < 0)
                {
                    var span = name.AsSpan();
                    int pos = 0;
                    while (pos < span.Length && !char.IsDigit(span[pos]))
                        pos++;
                    int start = pos;
                    while (pos < span.Length && char.IsDigit(span[pos]))
                        pos++;
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

    public async Task<bool> FinalizeReleaseAsync(
        string artistId,
        string releaseFolderName,
        CancellationToken ct
    )
    {
        var serviceLogger = await GetLogger();
        // Prepare per-release logger
        IDownloadLogger relLogger = new NullDownloadLogger();
        DownloadLogger? relLoggerImpl = null;
        try
        {
            var rel = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);
            if (rel != null)
            {
                var logPath = await logPathProvider.GetReleaseLogFilePathAsync(
                    rel.ArtistName,
                    rel.Title,
                    ct
                );
                if (!string.IsNullOrWhiteSpace(logPath))
                {
                    relLoggerImpl = new DownloadLogger(logPath!);
                    relLogger = relLoggerImpl;
                }
            }
        }
        catch { }

        var completedPath = options.Value.CompletedPath;
        if (string.IsNullOrWhiteSpace(completedPath) || !Directory.Exists(completedPath))
        {
            logger.LogDebug("[SAB Finalize] CompletedPath not configured/existing; skip");
            try
            {
                relLogger.Warn("[SAB Finalize] CompletedPath not configured/existing");
            }
            catch { }
            serviceLogger.Warn("[SAB Finalize] CompletedPath not configured/existing");
            return false;
        }

        var completed = completedPath!.TrimEnd(
            System.IO.Path.DirectorySeparatorChar,
            System.IO.Path.AltDirectorySeparatorChar
        );
        try
        {
            relLogger.Info($"[SAB Finalize] Using CompletedPath root: {completed}");
        }
        catch { }
        // Use the NZB name convention we submit to SAB: "{artistName} - {releaseTitle}"
        // Fetch release metadata to construct the expected job name and folder
        var relForNames = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);
        if (relForNames == null)
        {
            logger.LogWarning(
                "[SAB Finalize] Release not found in cache for {ArtistId}/{ReleaseFolderName}",
                artistId,
                releaseFolderName
            );
            return false;
        }
        var expectedNzbName = $"{relForNames.ArtistName} - {relForNames.Title}";
        var legacyNzbName = $"{artistId} - {releaseFolderName}";
        var sourceRoot = System.IO.Path.Combine(completed, expectedNzbName);
        if (!Directory.Exists(sourceRoot))
        {
            var alt = System.IO.Path.Combine(completed, legacyNzbName);
            if (Directory.Exists(alt))
                sourceRoot = alt;
        }
        logger.LogInformation("[SAB Finalize] Checking {SourceRoot}", sourceRoot);
        try
        {
            relLogger.Info($"[SAB Finalize] Checking {sourceRoot}");
        }
        catch { }
        serviceLogger.Info($"[SAB Finalize] Checking {sourceRoot}");

        // Check SABnzbd API to see if the job is actually complete
        var nzbName = expectedNzbName;
        logger.LogInformation("[SAB Finalize] Checking SABnzbd job status for: {NzbName}", nzbName);
        try
        {
            relLogger.Info($"[SAB Finalize] Checking SABnzbd job status for: {nzbName}");
        }
        catch { }
        serviceLogger.Info($"[SAB Finalize] Checking SABnzbd job status for: {nzbName}");

        // Wait a bit for SABnzbd to start processing, then check job status
        for (int attempt = 1; attempt <= 5; attempt++)
        {
            if (attempt > 1)
            {
                logger.LogInformation(
                    "[SAB Finalize] Attempt {Attempt}/5, waiting 15s before checking job status",
                    attempt
                );
                try
                {
                    relLogger.Info($"[SAB Finalize] Attempt {attempt}/5 waiting 15s");
                }
                catch { }
                await Task.Delay(TimeSpan.FromSeconds(15), ct);
            }

            logger.LogInformation(
                "[SAB Finalize] Attempt {Attempt}/5: Checking if job '{JobName}' is complete...",
                attempt,
                nzbName
            );

            // First check if SABnzbd says the job is complete
            var isJobComplete = await sabnzbdClient.IsJobCompleteAsync(nzbName, ct);
            logger.LogInformation(
                "[SAB Finalize] Attempt {Attempt}/5: IsJobCompleteAsync returned: {Result}",
                attempt,
                isJobComplete
            );

            if (!isJobComplete)
            {
                // Get detailed status for debugging
                var detailedStatus = await sabnzbdClient.GetJobStatusAsync(nzbName, ct);
                logger.LogInformation(
                    "[SAB Finalize] Attempt {Attempt}/5: Job not complete yet. Detailed status: {Status}",
                    attempt,
                    detailedStatus ?? "unknown"
                );
                continue;
            }

            logger.LogInformation(
                "[SAB Finalize] Attempt {Attempt}/5: SABnzbd reports job complete, attempting to finalize files",
                attempt
            );
            try
            {
                relLogger.Info($"[SAB Finalize] Attempt {attempt}/5: job complete; finalizing");
            }
            catch { }
            serviceLogger.Info($"[SAB Finalize] Attempt {attempt}/5: job complete; finalizing");

            // Job is complete, now try to finalize the files
            var result = await TryFinalizeReleaseCoreAsync(
                artistId,
                releaseFolderName,
                sourceRoot,
                relLogger,
                ct
            );
            if (result)
            {
                logger.LogInformation(
                    "[SAB Finalize] Successfully finalized on attempt {Attempt}",
                    attempt
                );
                try
                {
                    relLogger.Info($"[SAB Finalize] Success on attempt {attempt}");
                }
                catch { }
                serviceLogger.Info($"[SAB Finalize] Success on attempt {attempt}");
                return true; // SUCCESS - exit immediately, don't continue with more attempts
            }

            logger.LogWarning(
                "[SAB Finalize] Job complete but file finalization failed on attempt {Attempt}/5. This should not happen normally.",
                attempt
            );
            serviceLogger.Warn(
                $"[SAB Finalize] Job complete but file finalization failed on attempt {attempt}"
            );
            // Don't continue with more attempts if the job is complete but finalization failed
            // This likely indicates a filesystem issue that won't be resolved by waiting
            return false;
        }

        logger.LogInformation(
            "[SAB Finalize] All attempts failed, job may still be processing or files may be inaccessible"
        );
        try
        {
            relLogger.Warn("[SAB Finalize] All attempts failed");
        }
        catch { }
        return false;
    }

    private async Task<bool> TryFinalizeReleaseCoreAsync(
        string artistId,
        string releaseFolderName,
        string sourceRoot,
        IDownloadLogger relLogger,
        CancellationToken ct
    )
    {
        try
        {
            logger.LogInformation(
                "[SAB Finalize] Starting finalization for {ArtistId}/{ReleaseFolderName}",
                artistId,
                releaseFolderName
            );
            try
            {
                relLogger.Info(
                    $"[SAB Finalize] Starting finalization for {artistId}/{releaseFolderName}"
                );
            }
            catch { }
            logger.LogInformation("[SAB Finalize] Source root: {SourceRoot}", sourceRoot);
            try
            {
                relLogger.Info($"[SAB Finalize] Source root: {sourceRoot}");
            }
            catch { }

            var release = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);
            if (release == null)
            {
                logger.LogWarning(
                    "[SAB Finalize] Release not found in cache for {ArtistId}/{ReleaseFolderName}",
                    artistId,
                    releaseFolderName
                );
                return false;
            }

            var targetDir = release.ReleasePath;
            logger.LogInformation("[SAB Finalize] Target directory: {TargetDir}", targetDir);
            try
            {
                relLogger.Info($"[SAB Finalize] Target directory: {targetDir}");
            }
            catch { }

            if (string.IsNullOrWhiteSpace(targetDir))
            {
                logger.LogWarning(
                    "[SAB Finalize] Release path is null or empty for {ArtistId}/{ReleaseFolderName}",
                    artistId,
                    releaseFolderName
                );
                return false;
            }

            Directory.CreateDirectory(targetDir);
            logger.LogInformation(
                "[SAB Finalize] Created target directory: {TargetDir}",
                targetDir
            );
            try
            {
                relLogger.Info($"[SAB Finalize] Created target directory: {targetDir}");
            }
            catch { }

            if (!Directory.Exists(sourceRoot))
            {
                logger.LogWarning(
                    "[SAB Finalize] Source root directory does not exist: {SourceRoot}",
                    sourceRoot
                );
                return false;
            }

            logger.LogInformation(
                "[SAB Finalize] Source root directory exists, scanning for audio files..."
            );
            try
            {
                relLogger.Info("[SAB Finalize] Scanning for audio files in source root");
            }
            catch { }

            var sourceFiles = Directory
                .EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories)
                .Where(f =>
                    AudioExtensions.Contains(System.IO.Path.GetExtension(f).ToLowerInvariant())
                )
                .ToList();

            logger.LogInformation(
                "[SAB Finalize] Found {Count} audio files in source directory",
                sourceFiles.Count
            );
            try
            {
                relLogger.Info($"[SAB Finalize] Found {sourceFiles.Count} audio files");
            }
            catch { }

            if (sourceFiles.Count == 0)
            {
                logger.LogWarning(
                    "[SAB Finalize] No audio files found in source directory: {SourceRoot}",
                    sourceRoot
                );
                return false;
            }

            var movedAny = false;
            foreach (var src in sourceFiles)
            {
                try
                {
                    var fi1 = new FileInfo(src);
                    var size1 = fi1.Length;
                    await Task.Delay(1000, ct);
                    fi1.Refresh();
                    var size2 = fi1.Length;
                    if (size1 != size2)
                        continue;

                    var dest = System.IO.Path.Combine(targetDir, System.IO.Path.GetFileName(src));
                    var destFinal = dest;
                    int counter = 1;
                    while (File.Exists(destFinal))
                    {
                        var name = System.IO.Path.GetFileNameWithoutExtension(dest);
                        var ext = System.IO.Path.GetExtension(dest);
                        destFinal = System.IO.Path.Combine(targetDir, $"{name} ({counter++}){ext}");
                    }
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destFinal)!);
                    try
                    {
                        File.Move(src, destFinal);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        File.Copy(src, destFinal, overwrite: false);
                        try
                        {
                            File.Delete(src);
                        }
                        catch { }
                    }
                    catch (IOException)
                    {
                        File.Copy(src, destFinal, overwrite: false);
                        try
                        {
                            File.Delete(src);
                        }
                        catch { }
                    }
                    movedAny = true;
                    logger.LogInformation("[SAB Finalize] Moved {Src} -> {Dst}", src, destFinal);
                    try
                    {
                        relLogger.Info($"[SAB Finalize] Moved {src} -> {destFinal}");
                    }
                    catch { }
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "[SAB Finalize] Move failed for {Src}", src);
                }
            }
            logger.LogInformation(
                "[SAB Finalize] Successfully moved {Count} files",
                movedAny ? "some" : "0"
            );
            try
            {
                relLogger.Info($"[SAB Finalize] Moved any: {movedAny}");
            }
            catch { }

            if (!movedAny)
            {
                logger.LogWarning("[SAB Finalize] No files were moved successfully");
                return false;
            }

            // Update release.json
            var audioFiles = Directory
                .GetFiles(targetDir)
                .Where(f =>
                    AudioExtensions.Contains(System.IO.Path.GetExtension(f).ToLowerInvariant())
                )
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .Select(System.IO.Path.GetFileName)
                .ToList();

            var releaseJsonPath = System.IO.Path.Combine(targetDir, "release.json");
            if (File.Exists(releaseJsonPath))
            {
                using var scope = scopeFactory.CreateScope();
                var writer = scope.ServiceProvider.GetRequiredService<ServerLibraryJsonWriter>();
                await writer.UpdateReleaseAsync(
                    artistId,
                    releaseFolderName,
                    rel =>
                    {
                        // Helpers to extract disc and track numbers
                        static (int disc, int track) ExtractDiscTrack(string? name)
                        {
                            int disc = 1;
                            int track = -1;
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(name))
                                {
                                    var lower = name!.ToLowerInvariant();
                                    var m = System.Text.RegularExpressions.Regex.Match(
                                        lower,
                                        @"\b(?:cd|disc|disk|digital\s*media)\s*(\d+)\b",
                                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                                    );
                                    if (m.Success && int.TryParse(m.Groups[1].Value, out var d))
                                        disc = d;
                                    // Look for "- NN -" as embedded track number
                                    var m2 = System.Text.RegularExpressions.Regex.Match(
                                        name,
                                        @"-\s*(\d{1,3})\s*-",
                                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                                    );
                                    if (m2.Success && int.TryParse(m2.Groups[1].Value, out var t2))
                                        track = t2;
                                    if (track < 0)
                                    {
                                        // Fallback to leading digits
                                        var span = name.AsSpan();
                                        int pos = 0;
                                        while (pos < span.Length && !char.IsDigit(span[pos]))
                                            pos++;
                                        int start = pos;
                                        while (pos < span.Length && char.IsDigit(span[pos]))
                                            pos++;
                                        if (
                                            pos > start
                                            && int.TryParse(
                                                span.Slice(start, pos - start),
                                                out var n
                                            )
                                        )
                                        {
                                            track = n > 99 ? (n % 100 == 0 ? n : n % 100) : n;
                                        }
                                    }
                                }
                            }
                            catch { }
                            return (disc, track);
                        }

                        // Build map by disc->track->filename
                        var byDiscTrack = new Dictionary<int, Dictionary<int, string>>();
                        foreach (var f in audioFiles)
                        {
                            var (disc, track) = ExtractDiscTrack(f);
                            if (track <= 0)
                                continue;
                            if (!byDiscTrack.TryGetValue(disc, out var inner))
                            {
                                inner = new Dictionary<int, string>();
                                byDiscTrack[disc] = inner;
                            }
                            if (!inner.ContainsKey(track))
                                inner[track] = f;
                        }

                        // Update discs if present
                        if (rel.Discs is { Count: > 0 })
                        {
                            foreach (var d in rel.Discs)
                            {
                                if (d.Tracks == null)
                                    continue;
                                var discNum = d.DiscNumber > 0 ? d.DiscNumber : 1;
                                if (byDiscTrack.TryGetValue(discNum, out var inner))
                                {
                                    foreach (var t in d.Tracks)
                                    {
                                        if (
                                            t.TrackNumber > 0
                                            && inner.TryGetValue(t.TrackNumber, out var fname)
                                        )
                                        {
                                            t.AudioFilePath =
                                                "./" + System.IO.Path.GetFileName(fname);
                                        }
                                    }
                                }
                            }
                        }

                        // Also update flat tracks for compatibility
                        if (rel.Tracks is { Count: > 0 })
                        {
                            foreach (var t in rel.Tracks)
                            {
                                var discNum = t.DiscNumber ?? 1;
                                if (
                                    byDiscTrack.TryGetValue(discNum, out var inner)
                                    && inner.TryGetValue(t.TrackNumber, out var fname)
                                )
                                {
                                    t.AudioFilePath = "./" + System.IO.Path.GetFileName(fname);
                                }
                            }
                        }
                    }
                );
            }

            await cache.UpdateReleaseFromJsonAsync(artistId, releaseFolderName);

            // Mark availability using disc-aware mapping
            try
            {
                if (File.Exists(releaseJsonPath))
                {
                    // Build a map from disc->track->file again to drive status updates
                    var byDiscTrack = new Dictionary<int, HashSet<int>>();
                    foreach (var f in audioFiles)
                    {
                        var (disc, track) =
                            MusicGQL.Features.Downloads.Util.FileNameParsing.ExtractDiscTrackFromName(
                                f
                            );
                        if (track <= 0)
                            continue;
                        if (!byDiscTrack.TryGetValue(disc, out var set))
                        {
                            set = new HashSet<int>();
                            byDiscTrack[disc] = set;
                        }
                        set.Add(track);
                    }

                    foreach (var (disc, set) in byDiscTrack)
                    {
                        foreach (var track in set)
                        {
                            await cache.UpdateMediaAvailabilityStatus(
                                artistId,
                                releaseFolderName,
                                disc,
                                track,
                                CachedMediaAvailabilityStatus.Available
                            );
                        }
                    }
                }
                else
                {
                    // Fallback: sequential marking
                    await Task.WhenAll(
                        audioFiles.Select(
                            (_, i) =>
                                cache.UpdateMediaAvailabilityStatus(
                                    artistId,
                                    releaseFolderName,
                                    i + 1,
                                    CachedMediaAvailabilityStatus.Available
                                )
                        )
                    );
                }
            }
            catch { }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "[SAB Finalize] Finalize failed for {ArtistId}/{Release}",
                artistId,
                releaseFolderName
            );
            try
            {
                relLogger.Error($"[SAB Finalize] Exception: {ex.Message}");
            }
            catch { }
            return false;
        }
    }
}
