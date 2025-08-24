using System.Text.RegularExpressions;
using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ServerLibrary.Writer;
using MusicGQL.Features.ServerSettings;
using MusicGQL.Integration.MusicBrainz;
using Path = System.IO.Path;

namespace MusicGQL.Features.Downloads.Services;

public class DiscographyImporter(
    ILogger<DiscographyImporter> logger,
    ServerSettingsAccessor serverSettingsAccessor,
    ServerLibraryCache cache,
    ServerLibraryJsonWriter jsonWriter,
    ReleaseJsonBuilder releaseJsonBuilder,
    MusicBrainzService musicBrainzService,
    MediaFileAssignmentService mediaAssigner,
    IFolderIdentityService folderIdentity
)
{
    private static readonly string[] AudioExts = new[]
    {
        ".mp3",
        ".flac",
        ".m4a",
        ".wav",
        ".ogg",
        ".aac",
    };

    public async Task<bool> TryImportDiscographyFolderAsync(
        string discographyFolderPath,
        CancellationToken cancellationToken
    )
    {
        try
        {
            if (!Directory.Exists(discographyFolderPath))
                return false;

            var folderName = Path.GetFileName(
                discographyFolderPath.TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar
                )
            );
            if (string.IsNullOrWhiteSpace(folderName))
                return false;

            // Guess artist by folder name
            var artist = await GuessArtistAsync(folderName);
            if (artist is null)
            {
                logger.LogInformation(
                    "[DiscographyImporter] Could not match artist for folder '{Folder}'",
                    folderName
                );
                return false;
            }

            var artistId = artist.Id;
            var artistName = artist.JsonArtist.Name;
            var mbArtistId = artist.JsonArtist.Connections?.MusicBrainzArtistId;
            if (string.IsNullOrWhiteSpace(mbArtistId))
            {
                logger.LogInformation(
                    "[DiscographyImporter] Skipping '{Folder}' - library artist lacks MusicBrainz ID",
                    folderName
                );
                return false;
            }

            // Treat immediate subfolders as candidate releases
            var releases = SafeEnumerateDirectories(discographyFolderPath).ToList();
            if (releases.Count == 0)
            {
                // Some discographies may drop files directly; treat as single release
                releases = [discographyFolderPath];
            }

            int imported = 0;
            foreach (var releaseFolder in releases)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var releaseTitle = Path.GetFileName(releaseFolder);
                if (string.IsNullOrWhiteSpace(releaseTitle))
                    continue;

                // Skip non-audio folders (no audio files within)
                var anyAudio = SafeEnumerateFiles(releaseFolder)
                    .Any(f =>
                        AudioExts.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase)
                    );
                if (!anyAudio)
                {
                    continue;
                }

                var targetFolderName = releaseTitle.Trim();
                var already = await cache.GetReleaseByArtistAndFolderAsync(
                    artistId,
                    targetFolderName
                );
                if (already != null)
                {
                    logger.LogInformation(
                        "[DiscographyImporter] Release already exists in library: {Artist}/{Release}",
                        artistId,
                        targetFolderName
                    );
                    continue;
                }

                // Use audio-aware identification to pick the best release group for this folder
                var id = await folderIdentity.IdentifyReleaseAsync(
                    artistName,
                    mbArtistId,
                    releaseFolder
                );
                if (id is null)
                {
                    logger.LogInformation(
                        "[DiscographyImporter] No MusicBrainz match for '{ReleaseTitle}' (artist: {Artist})",
                        releaseTitle,
                        artistName
                    );
                    continue;
                }

                var matchedRgId = id.ReleaseGroupId;
                var primaryType = id.PrimaryType;
                var resolvedTitle = string.IsNullOrWhiteSpace(id.Title) ? releaseTitle : id.Title;

                // Build release.json under library using the chosen RG and local audio context
                var artistDir = Path.Combine(
                    (await serverSettingsAccessor.GetAsync()).LibraryPath,
                    artistId
                );
                Directory.CreateDirectory(artistDir);
                var json = await releaseJsonBuilder.BuildAsync(
                    artistDir,
                    matchedRgId,
                    targetFolderName,
                    resolvedTitle,
                    primaryType: primaryType
                );
                if (json is null)
                {
                    logger.LogInformation(
                        "[DiscographyImporter] Failed building release.json for '{ReleaseTitle}'",
                        releaseTitle
                    );
                    continue;
                }

                await jsonWriter.WriteReleaseAsync(artistId, targetFolderName, json);

                // Copy audio files into library release dir
                var libReleaseDir = Path.Combine(artistDir, targetFolderName);
                Directory.CreateDirectory(libReleaseDir);
                foreach (var file in SafeEnumerateFiles(releaseFolder))
                {
                    try
                    {
                        if (
                            !AudioExts.Contains(
                                Path.GetExtension(file),
                                StringComparer.OrdinalIgnoreCase
                            )
                        )
                            continue;
                        var dest = Path.Combine(libReleaseDir, Path.GetFileName(file));
                        if (!File.Exists(dest))
                        {
                            File.Copy(file, dest, overwrite: false);
                        }
                    }
                    catch { }
                }

                // Assign media to tracks and update cache
                await mediaAssigner.AssignAsync(artistId, targetFolderName);
                await cache.UpdateReleaseFromJsonAsync(artistId, targetFolderName);
                imported++;
                logger.LogInformation(
                    "[DiscographyImporter] Imported '{ReleaseTitle}' → {Artist}/{Release}",
                    releaseTitle,
                    artistId,
                    targetFolderName
                );
            }

            logger.LogInformation(
                "[DiscographyImporter] Completed import from '{Folder}'. Imported={Count}",
                folderName,
                imported
            );
            return imported > 0;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "[DiscographyImporter] Failed processing folder '{Folder}'",
                discographyFolderPath
            );
            return false;
        }
    }

    private async Task<CachedArtist?> GuessArtistAsync(string folderName)
    {
        // Prefer exact name match; fallback to contains
        var all = await cache.GetAllArtistsAsync();
        var exact = all.FirstOrDefault(a =>
            string.Equals(a.Name, folderName, StringComparison.OrdinalIgnoreCase)
        );
        if (exact != null)
            return exact;
        var contains = all.FirstOrDefault(a =>
            a.Name.Contains(folderName, StringComparison.OrdinalIgnoreCase)
            || folderName.Contains(a.Name, StringComparison.OrdinalIgnoreCase)
        );
        return contains;
    }

    private static IEnumerable<string> SafeEnumerateDirectories(string root)
    {
        try
        {
            return Directory.EnumerateDirectories(root);
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private static IEnumerable<string> SafeEnumerateFiles(string root)
    {
        try
        {
            return Directory.EnumerateFiles(root, "*", SearchOption.TopDirectoryOnly);
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
}
