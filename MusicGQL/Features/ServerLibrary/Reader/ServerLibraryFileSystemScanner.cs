using System.Text.Json;
using System.Text.Json.Serialization;
using MusicGQL.Features.ServerSettings;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Reader;

/// <summary>
/// Scans the library folder for artist/release folders that contain audio files but
/// are missing their corresponding JSON metadata files. When found, imports metadata
/// from MusicBrainz and creates the JSON files in-place, then refreshes the cache.
/// </summary>
public class ServerLibraryFileSystemScanner(ServerSettingsAccessor serverSettingsAccessor)
{
    private async Task<string> GetLibraryPathAsync()
    {
        try
        {
            var s = await serverSettingsAccessor.GetAsync();
            return s.LibraryPath;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static readonly string[] AudioExtensions = [".mp3", ".flac", ".wav", ".m4a", ".ogg"];

    public class ScanResult
    {
        public bool Success { get; set; }
        public List<string> Notes { get; set; } = [];
        public string? ErrorMessage { get; set; }
    }

    public record ReleaseFolderScan(
        string ArtistDir,
        string ReleaseDir,
        bool HasAudio,
        bool MissingReleaseJson
    );

    public record ArtistFolderScan(
        string ArtistDir,
        bool MissingArtistJson,
        List<ReleaseFolderScan> Releases
    );

    public class ScanPlan
    {
        public List<ArtistFolderScan> ArtistsNeedingWork { get; set; } = [];
    }

    // Legacy method kept temporarily for compatibility; now only scans and reports
    public async Task<ScanResult> ScanAndFixMissingMetadataAsync()
    {
        var result = new ScanResult();

        try
        {
            var libraryPath = await GetLibraryPathAsync();
            if (string.IsNullOrWhiteSpace(libraryPath) || !Directory.Exists(libraryPath))
            {
                result.Success = true;
                result.Notes.Add($"Library path not found: {Path.GetFullPath(libraryPath ?? string.Empty)}");
                return result;
            }

            var plan = await ScanAsync();
            foreach (var artist in plan.ArtistsNeedingWork)
            {
                var artistFolderName = Path.GetFileName(artist.ArtistDir) ?? "";
                if (artist.MissingArtistJson)
                {
                    result.Notes.Add($"Artist '{artistFolderName}' is missing artist.json");
                }

                foreach (var rel in artist.Releases)
                {
                    if (rel.MissingReleaseJson)
                    {
                        result.Notes.Add(
                            $"Release '{Path.GetFileName(rel.ReleaseDir)}' under '{artistFolderName}' is missing release.json"
                        );
                    }
                }
            }

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public async Task<ScanPlan> ScanAsync()
    {
        var plan = new ScanPlan();

        var libraryPath = await GetLibraryPathAsync();
        if (string.IsNullOrWhiteSpace(libraryPath) || !Directory.Exists(libraryPath))
        {
            return plan;
        }

        var artistDirectories = Directory.GetDirectories(libraryPath);
        foreach (var artistDir in artistDirectories)
        {
            var releaseDirs = Directory.GetDirectories(artistDir);
            var releaseScans = new List<ReleaseFolderScan>();
            var hasAnyAudio = false;
            foreach (var releaseDir in releaseDirs)
            {
                var hasAudio = ContainsAnyAudioFile(releaseDir);
                var missingReleaseJson =
                    hasAudio && !File.Exists(Path.Combine(releaseDir, "release.json"));
                if (hasAudio)
                {
                    hasAnyAudio = true;
                    releaseScans.Add(
                        new ReleaseFolderScan(artistDir, releaseDir, hasAudio, missingReleaseJson)
                    );
                }
            }

            if (!hasAnyAudio)
            {
                continue;
            }

            var missingArtistJson = !File.Exists(Path.Combine(artistDir, "artist.json"));
            var artistScan = new ArtistFolderScan(artistDir, missingArtistJson, releaseScans);
            // only add if artist.json is missing or any release json is missing
            if (missingArtistJson || releaseScans.Any(r => r.MissingReleaseJson))
            {
                plan.ArtistsNeedingWork.Add(artistScan);
            }
        }

        return plan;
    }

    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };
    }

    private static bool ContainsAnyAudioFile(string directory)
    {
        if (!Directory.Exists(directory))
            return false;

        try
        {
            // only scan immediate files within the release folder
            var files = Directory.GetFiles(directory);
            return files.Any(f =>
                AudioExtensions.Contains(Path.GetExtension(f).ToLowerInvariant())
            );
        }
        catch
        {
            return false;
        }
    }
}
