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
    IServiceScopeFactory scopeFactory
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
        var sourceRoot = System.IO.Path.Combine(completed, "mmusic", artistId, releaseFolderName);
        logger.LogInformation("[SAB Finalize] Checking {SourceRoot}", sourceRoot);
        return await TryFinalizeReleaseCoreAsync(artistId, releaseFolderName, sourceRoot, ct);
    }

    private async Task<bool> TryFinalizeReleaseCoreAsync(string artistId, string releaseFolderName, string sourceRoot, CancellationToken ct)
    {
        try
        {
            var release = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);
            if (release == null) return false;
            var targetDir = release.ReleasePath;
            if (string.IsNullOrWhiteSpace(targetDir)) return false;
            Directory.CreateDirectory(targetDir);
            if (!Directory.Exists(sourceRoot)) return false;

            var sourceFiles = System.IO.Directory
                .EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories)
                .Where(f => AudioExtensions.Contains(System.IO.Path.GetExtension(f).ToLowerInvariant()))
                .ToList();
            if (sourceFiles.Count == 0) return false;

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
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destFinal)!);
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
            if (!movedAny) return false;

            // Update release.json
            var audioFiles = System.IO.Directory
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


