using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MusicGQL.Features.External.Downloads.Sabnzbd.Configuration;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Writer;
using System.IO;

namespace MusicGQL.Features.External.Downloads.Sabnzbd;

public class SabnzbdFinalizeService(
    IOptions<SabnzbdOptions> options,
    ILogger<SabnzbdFinalizeService> logger,
    ServerLibraryCache cache,
    ServerSettings.ServerSettingsAccessor serverSettingsAccessor,
    IServiceScopeFactory scopeFactory,
    SabnzbdClient sabnzbdClient
)
{
    private static readonly string[] AudioExtensions = new[] { ".mp3", ".flac", ".m4a", ".wav", ".ogg" };

    public async Task<bool> FinalizeReleaseAsync(string artistId, string releaseFolderName, CancellationToken ct)
    {
        var completedPath = options.Value.CompletedPath;
        if (string.IsNullOrWhiteSpace(completedPath) || !Directory.Exists(completedPath))
        {
            logger.LogDebug("[SAB Finalize] CompletedPath not configured/existing; skip");
            return false;
        }

        var completed = completedPath!.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
        // Use the NZB name convention we submit to SAB: "{artistName} - {releaseTitle}"
        // Fetch release metadata to construct the expected job name and folder
        var relForNames = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);
        if (relForNames == null)
        {
            logger.LogWarning("[SAB Finalize] Release not found in cache for {ArtistId}/{ReleaseFolderName}", artistId, releaseFolderName);
            return false;
        }
        var expectedNzbName = $"{relForNames.ArtistName} - {relForNames.Title}";
        var legacyNzbName = $"{artistId} - {releaseFolderName}";
        var sourceRoot = System.IO.Path.Combine(completed, expectedNzbName);
        if (!Directory.Exists(sourceRoot))
        {
            var alt = System.IO.Path.Combine(completed, legacyNzbName);
            if (Directory.Exists(alt)) sourceRoot = alt;
        }
        logger.LogInformation("[SAB Finalize] Checking {SourceRoot}", sourceRoot);

        // Check SABnzbd API to see if the job is actually complete
        var nzbName = expectedNzbName;
        logger.LogInformation("[SAB Finalize] Checking SABnzbd job status for: {NzbName}", nzbName);
        
        // Wait a bit for SABnzbd to start processing, then check job status
        for (int attempt = 1; attempt <= 5; attempt++)
        {
            if (attempt > 1)
            {
                logger.LogInformation("[SAB Finalize] Attempt {Attempt}/5, waiting 15s before checking job status", attempt);
                await Task.Delay(TimeSpan.FromSeconds(15), ct);
            }
            
            logger.LogInformation("[SAB Finalize] Attempt {Attempt}/5: Checking if job '{JobName}' is complete...", attempt, nzbName);
            
            // First check if SABnzbd says the job is complete
            var isJobComplete = await sabnzbdClient.IsJobCompleteAsync(nzbName, ct);
            logger.LogInformation("[SAB Finalize] Attempt {Attempt}/5: IsJobCompleteAsync returned: {Result}", attempt, isJobComplete);
            
            if (!isJobComplete)
            {
                // Get detailed status for debugging
                var detailedStatus = await sabnzbdClient.GetJobStatusAsync(nzbName, ct);
                logger.LogInformation("[SAB Finalize] Attempt {Attempt}/5: Job not complete yet. Detailed status: {Status}", attempt, detailedStatus ?? "unknown");
                continue;
            }
            
            logger.LogInformation("[SAB Finalize] Attempt {Attempt}/5: SABnzbd reports job complete, attempting to finalize files", attempt);
            
            // Job is complete, now try to finalize the files
            var result = await TryFinalizeReleaseCoreAsync(artistId, releaseFolderName, sourceRoot, ct);
            if (result)
            {
                logger.LogInformation("[SAB Finalize] Successfully finalized on attempt {Attempt}", attempt);
                return true; // SUCCESS - exit immediately, don't continue with more attempts
            }
            
            logger.LogWarning("[SAB Finalize] Job complete but file finalization failed on attempt {Attempt}/5. This should not happen normally.", attempt);
            // Don't continue with more attempts if the job is complete but finalization failed
            // This likely indicates a filesystem issue that won't be resolved by waiting
            return false;
        }
        
        logger.LogInformation("[SAB Finalize] All attempts failed, job may still be processing or files may be inaccessible");
        return false;
    }

    private async Task<bool> TryFinalizeReleaseCoreAsync(string artistId, string releaseFolderName, string sourceRoot, CancellationToken ct)
    {
        try
        {
            logger.LogInformation("[SAB Finalize] Starting finalization for {ArtistId}/{ReleaseFolderName}", artistId, releaseFolderName);
            logger.LogInformation("[SAB Finalize] Source root: {SourceRoot}", sourceRoot);
            
            var release = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);
            if (release == null)
            {
                logger.LogWarning("[SAB Finalize] Release not found in cache for {ArtistId}/{ReleaseFolderName}", artistId, releaseFolderName);
                return false;
            }
            
            var targetDir = release.ReleasePath;
            logger.LogInformation("[SAB Finalize] Target directory: {TargetDir}", targetDir);
            
            if (string.IsNullOrWhiteSpace(targetDir))
            {
                logger.LogWarning("[SAB Finalize] Release path is null or empty for {ArtistId}/{ReleaseFolderName}", artistId, releaseFolderName);
                return false;
            }
            
            Directory.CreateDirectory(targetDir);
            logger.LogInformation("[SAB Finalize] Created target directory: {TargetDir}", targetDir);
            
            if (!Directory.Exists(sourceRoot))
            {
                logger.LogWarning("[SAB Finalize] Source root directory does not exist: {SourceRoot}", sourceRoot);
                return false;
            }
            
            logger.LogInformation("[SAB Finalize] Source root directory exists, scanning for audio files...");
            
            var sourceFiles = Directory
                .EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories)
                .Where(f => AudioExtensions.Contains(System.IO.Path.GetExtension(f).ToLowerInvariant()))
                .ToList();
                
            logger.LogInformation("[SAB Finalize] Found {Count} audio files in source directory", sourceFiles.Count);
            
            if (sourceFiles.Count == 0)
            {
                logger.LogWarning("[SAB Finalize] No audio files found in source directory: {SourceRoot}", sourceRoot);
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
                    if (size1 != size2) continue;

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
                    try { File.Move(src, destFinal); }
                    catch (UnauthorizedAccessException) { File.Copy(src, destFinal, overwrite: false); try { File.Delete(src); } catch { } }
                    catch (IOException) { File.Copy(src, destFinal, overwrite: false); try { File.Delete(src); } catch { } }
                    movedAny = true;
                    logger.LogInformation("[SAB Finalize] Moved {Src} -> {Dst}", src, destFinal);
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "[SAB Finalize] Move failed for {Src}", src);
                }
            }
            logger.LogInformation("[SAB Finalize] Successfully moved {Count} files", movedAny ? "some" : "0");
            
            if (!movedAny)
            {
                logger.LogWarning("[SAB Finalize] No files were moved successfully");
                return false;
            }

            // Update release.json
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
                await writer.UpdateReleaseAsync(artistId, releaseFolderName, rel =>
                {
                    if (rel.Tracks is null) return;
                    static int ExtractLeadingNumber(string? name)
                    {
                        if (string.IsNullOrWhiteSpace(name)) return -1;
                        var span = name.AsSpan();
                        int pos = 0; while (pos < span.Length && !char.IsDigit(span[pos])) pos++;
                        int start = pos; while (pos < span.Length && char.IsDigit(span[pos])) pos++;
                        if (pos > start && int.TryParse(span.Slice(start, pos - start), out var n))
                        { if (n > 99) { var lastTwo = n % 100; if (lastTwo > 0) return lastTwo; } return n; }
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
                        if (byTrackNo.TryGetValue(t.TrackNumber, out var fname)) t.AudioFilePath = "./" + fname;
                    }
                });
            }

            await cache.UpdateReleaseFromJsonAsync(artistId, releaseFolderName);
            await Task.WhenAll(audioFiles.Select((_, i) => cache.UpdateMediaAvailabilityStatus(
                artistId, releaseFolderName, i + 1, CachedMediaAvailabilityStatus.Available)));
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[SAB Finalize] Finalize failed for {ArtistId}/{Release}", artistId, releaseFolderName);
            return false;
        }
    }
}


