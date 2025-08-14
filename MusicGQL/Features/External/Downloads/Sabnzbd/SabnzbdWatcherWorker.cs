using Microsoft.Extensions.Options;
using MusicGQL.Features.External.Downloads.Sabnzbd.Configuration;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerSettings;
using MusicGQL.Features.ServerLibrary.Writer;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.IO;

namespace MusicGQL.Features.External.Downloads.Sabnzbd;

public class SabnzbdWatcherWorker(
    IOptions<SabnzbdOptions> options,
    ILogger<SabnzbdWatcherWorker> logger,
    ServerLibraryCache cache,
    ServerSettingsAccessor serverSettingsAccessor,
    IServiceScopeFactory scopeFactory
) : BackgroundService
{
    private static readonly string[] AudioExtensions = new[] { ".mp3", ".flac", ".m4a", ".wav", ".ogg" };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var completedPath = options.Value.CompletedPath;
        if (string.IsNullOrWhiteSpace(completedPath) || !Directory.Exists(completedPath))
        {
            logger.LogInformation("[SAB Watcher] CompletedPath not set or does not exist; watcher disabled");
            return;
        }

        logger.LogInformation("[SAB Watcher] Monitoring completed path: {Path}", completedPath);

        using var watcher = new FileSystemWatcher(completedPath)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size | NotifyFilters.LastWrite
        };

        var queue = new ConcurrentQueue<string>();
        var lastScanByKey = new ConcurrentDictionary<string, DateTime>();
        DateTime lastFullScan = DateTime.MinValue;
        void Enqueue(string p)
        {
            try { if (!string.IsNullOrWhiteSpace(p)) queue.Enqueue(p); } catch { }
        }

        watcher.Created += (_, e) => Enqueue(e.FullPath);
        watcher.Changed += (_, e) => Enqueue(e.FullPath);
        watcher.Renamed += (_, e) => Enqueue(e.FullPath);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                if (!queue.TryDequeue(out var path)) continue;
                // Heuristic: look for audio files appearing; when they do, trigger a rescan of artist/release if we can infer it from path later.
                var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
                if (ext is not (".mp3" or ".flac" or ".m4a" or ".wav" or ".ogg"))
                {
                    continue;
                }

                logger.LogInformation("[SAB Watcher] Detected new audio: {Path}", path);

                // Try to parse relative release folder from CompletedPath: expect path under CompletedPath/mmusic/{artistId}/{releaseFolderName}/...
                try
                {
                    var settings = await serverSettingsAccessor.GetAsync();
                    var completed = completedPath!.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
                    var rel = System.IO.Path.GetRelativePath(completed, path);
                    var parts = rel.Split(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
                    var idx = Array.FindIndex(parts, p => string.Equals(p, "mmusic", StringComparison.OrdinalIgnoreCase));
                    if (idx >= 0 && parts.Length >= idx + 3)
                    {
                        var artistId = parts[idx + 1];
                        var releaseFolder = parts[idx + 2];
                        logger.LogInformation("[SAB Watcher] Mapped to artistId={ArtistId}, releaseFolder={Release}", artistId, releaseFolder);

                        var sourceRoot = System.IO.Path.Combine(completed, "mmusic", artistId, releaseFolder);
                        logger.LogInformation("[SAB Watcher] Finalize check for {ArtistId}/{Release} from {SourceRoot}", artistId, releaseFolder, sourceRoot);
                        await TryFinalizeReleaseAsync(artistId, releaseFolder, sourceRoot, stoppingToken);
                        lastScanByKey[$"{artistId}|{releaseFolder}"] = DateTime.UtcNow;
                    }
                    else
                    {
                        // Fallback inference: try to match by folder name tokens (e.g., "D.A.D.-Monster Philosophy-2008-iFA")
                        var parent = System.IO.Path.GetDirectoryName(path);
                        if (!string.IsNullOrWhiteSpace(parent))
                        {
                            var inferred = await InferReleaseFromFolderAsync(parent!);
                            if (inferred != null)
                            {
                                var (artistId, relFolder) = inferred.Value;
                                logger.LogInformation("[SAB Watcher] Inferred mapping to {ArtistId}/{Release}", artistId, relFolder);
                                await TryFinalizeReleaseAsync(artistId, relFolder, parent!, stoppingToken);
                                lastScanByKey[$"{artistId}|{relFolder}"] = DateTime.UtcNow;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "[SAB Watcher] Path mapping failed");
                }

                // Opportunistic periodic sweep to catch missed events
                if ((DateTime.UtcNow - lastFullScan) > TimeSpan.FromSeconds(15))
                {
                    lastFullScan = DateTime.UtcNow;
                    try
                    {
                        var mmusicRoot = System.IO.Path.Combine(completedPath!, "mmusic");
                        if (Directory.Exists(mmusicRoot))
                        {
                            foreach (var artistDir in Directory.EnumerateDirectories(mmusicRoot))
                            {
                                foreach (var releaseDir in Directory.EnumerateDirectories(artistDir))
                                {
                                    var key = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(releaseDir)) + "|" + System.IO.Path.GetFileName(releaseDir);
                                    if (lastScanByKey.TryGetValue(key, out var ts) && (DateTime.UtcNow - ts) < TimeSpan.FromSeconds(30))
                                    {
                                        continue;
                                    }
                                    var artistId = System.IO.Path.GetFileName(artistDir);
                                    var releaseFolder = System.IO.Path.GetFileName(releaseDir);
                                    logger.LogDebug("[SAB Watcher] Sweep finalize {ArtistId}/{Release} from {ReleaseDir}", artistId, releaseFolder, releaseDir);
                                    await TryFinalizeReleaseAsync(artistId, releaseFolder, releaseDir, stoppingToken);
                                    lastScanByKey[key] = DateTime.UtcNow;
                                }
                            }
                        }

                        // Also sweep top-level immediate subfolders for inference if they contain audio files
                        foreach (var folder in Directory.EnumerateDirectories(completedPath!))
                        {
                            if (string.Equals(System.IO.Path.GetFileName(folder), "mmusic", StringComparison.OrdinalIgnoreCase))
                                continue;
                            try
                            {
                                var hasAudio = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
                                    .Any(f => AudioExtensions.Contains(System.IO.Path.GetExtension(f).ToLowerInvariant()));
                                if (!hasAudio) continue;
                                var inferred = await InferReleaseFromFolderAsync(folder);
                                if (inferred != null)
                                {
                                    var (artistId, relFolder) = inferred.Value;
                                    var key = artistId + "|" + relFolder;
                                    if (lastScanByKey.TryGetValue(key, out var ts2) && (DateTime.UtcNow - ts2) < TimeSpan.FromSeconds(30))
                                        continue;
                                    await TryFinalizeReleaseAsync(artistId, relFolder, folder, stoppingToken);
                                    lastScanByKey[key] = DateTime.UtcNow;
                                }
                            }
                            catch { }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogDebug(ex, "[SAB Watcher] Periodic sweep failed");
                    }
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[SAB Watcher] Error in loop");
            }
        }
    }

    private async Task TryFinalizeReleaseAsync(string artistId, string releaseFolderName, string sourceRoot, CancellationToken ct)
    {
        try
        {
            var release = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);
            if (release == null)
            {
                logger.LogWarning("[SAB Watcher] Release not found in cache for {ArtistId}/{Release}", artistId, releaseFolderName);
                return;
            }

            var targetDir = release.ReleasePath;
            if (string.IsNullOrWhiteSpace(targetDir)) return;
            Directory.CreateDirectory(targetDir);

            if (!Directory.Exists(sourceRoot)) return;

            // Collect audio files recursively
            var sourceFiles = Directory
                .EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories)
                .Where(f => AudioExtensions.Contains(System.IO.Path.GetExtension(f).ToLowerInvariant()))
                .ToList();

            if (sourceFiles.Count == 0)
            {
                logger.LogDebug("[SAB Watcher] No audio found under {SourceRoot}", sourceRoot);
                return;
            }

            // Attempt to move files; if locked, skip and retry on next event
            var movedAny = false;
            foreach (var src in sourceFiles)
            {
                try
                {
                    // Basic readiness check: make sure size stable for a short window
                    var fi1 = new FileInfo(src);
                    var size1 = fi1.Length;
                    await Task.Delay(1000, ct);
                    fi1.Refresh();
                    var size2 = fi1.Length;
                    if (size1 != size2)
                    {
                        continue; // still growing
                    }

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
                        // Fall back to copy if move isn't allowed (e.g., read-only source)
                        File.Copy(src, destFinal, overwrite: false);
                        try { File.Delete(src); } catch { }
                    }
                    catch (IOException)
                    {
                        // Cross-device or locked: try copy then delete
                        File.Copy(src, destFinal, overwrite: false);
                        try { File.Delete(src); } catch { }
                    }
                    movedAny = true;
                    logger.LogInformation("[SAB Watcher] Moved {File} -> {Dest}", src, destFinal);
                }
                catch (IOException)
                {
                    // Likely still being written; try later on next event
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "[SAB Watcher] Move failed for {File}", src);
                }
            }

            if (!movedAny)
            {
                return;
            }

            // Update release.json with audio file paths now present in targetDir
            var audioFiles = Directory
                .GetFiles(targetDir)
                .Where(f => AudioExtensions.Contains(System.IO.Path.GetExtension(f).ToLowerInvariant()))
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
                        if (rel.Tracks is null) return;

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
            }

            // Refresh cache and mark availability
            await cache.UpdateReleaseFromJsonAsync(artistId, releaseFolderName);
            await Task.WhenAll(
                audioFiles.Select((_, i) => cache.UpdateMediaAvailabilityStatus(
                    artistId,
                    releaseFolderName,
                    i + 1,
                    CachedMediaAvailabilityStatus.Available))
            );
            logger.LogInformation("[SAB Watcher] Finalized {ArtistId}/{Release}; {Count} audio files ready", artistId, releaseFolderName, audioFiles.Count);

            // Best-effort: clean up empty source folder
            try
            {
                if (Directory.Exists(sourceRoot) && !Directory.EnumerateFileSystemEntries(sourceRoot).Any())
                {
                    Directory.Delete(sourceRoot, true);
                }
            }
            catch { }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[SAB Watcher] Finalize failed for {ArtistId}/{Release}", artistId, releaseFolderName);
        }
    }

    private static string NormalizeTitle(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var s = input.Replace("’", "'").Replace("“", "\"").Replace("”", "\"");
        var builder = new System.Text.StringBuilder(s.Length);
        foreach (var ch in s)
        {
            if (char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch)) builder.Append(char.ToLowerInvariant(ch));
        }
        return System.Text.RegularExpressions.Regex.Replace(builder.ToString(), "\\s+", " ").Trim();
    }

    private static bool ContainsAllTokens(string haystack, params string[] tokens)
    {
        foreach (var t in tokens)
        {
            if (string.IsNullOrWhiteSpace(t)) return false;
            if (haystack.IndexOf(t, StringComparison.Ordinal) < 0) return false;
        }
        return true;
    }

    private async Task<(string ArtistId, string ReleaseFolderName)?> InferReleaseFromFolderAsync(string folder)
    {
        try
        {
            var name = System.IO.Path.GetFileName(folder);
            if (string.IsNullOrWhiteSpace(name)) return null;
            var norm = NormalizeTitle(name);
            var releases = await cache.GetAllReleasesAsync();
            (string A, string R)? best = null;
            int bestScore = 0;
            foreach (var rel in releases)
            {
                var a = NormalizeTitle(rel.ArtistName ?? string.Empty);
                var r = NormalizeTitle(rel.Title ?? string.Empty);
                int score = 0;
                if (!string.IsNullOrWhiteSpace(a) && norm.Contains(a)) score += a.Length;
                if (!string.IsNullOrWhiteSpace(r) && norm.Contains(r)) score += r.Length;
                if (score > bestScore && score >= Math.Min(norm.Length / 3, 12))
                {
                    bestScore = score;
                    best = (rel.ArtistId, rel.FolderName);
                }
            }
            return best;
        }
        catch
        {
            return null;
        }
    }
}


