using MusicGQL.Features.ServerSettings.Db;
using System.IO;
using System.Runtime.InteropServices;
using MusicGQL.Features.ServerLibrary;

namespace MusicGQL.Features.ServerSettings;

public record ServerSettings([property: GraphQLIgnore] DbServerSettings Model)
{
    [ID]
    public int Id() => Model.Id;

    public string LibraryPath() => Model.LibraryPath;

    public string DownloadPath() => Model.DownloadPath;

    public int SoulSeekSearchTimeLimitSeconds() => Model.SoulSeekSearchTimeLimitSeconds;

    public int SoulSeekNoDataTimeoutSeconds() => Model.SoulSeekNoDataTimeoutSeconds;

    public int DownloadSlotCount() => Model.DownloadSlotCount;

    public string ListenBrainzUsername() => Model.ListenBrainzUsername;

    public string ListenBrainzApiKey() => Model.ListenBrainzApiKey;

    // Top Tracks Service Configuration
    public bool ListenBrainzTopTracksEnabled() => Model.ListenBrainzTopTracksEnabled;

    public bool SpotifyTopTracksEnabled() => Model.SpotifyTopTracksEnabled;

    public bool LastFmTopTracksEnabled() => Model.LastFmTopTracksEnabled;

    public ServerLibraryManifestStatus ServerLibraryManifestStatus() => new();

    [GraphQLName("storageStats")]
    public async Task<StorageStats?> GetStorageStats([Service] ServerSettingsAccessor serverSettingsAccessor)
    {
        var settings = await serverSettingsAccessor.GetAsync();
        var libraryPath = settings.LibraryPath;
        if (string.IsNullOrWhiteSpace(libraryPath) || !Directory.Exists(libraryPath))
            return null;

        long? totalDiskBytes = null;
        long? availableFreeBytes = null;
        try
        {
            var root = System.IO.Path.GetPathRoot(libraryPath);
            if (!string.IsNullOrEmpty(root))
            {
                var drive = new DriveInfo(root);
                // On Unix-like systems, root will be "/" which is fine
                totalDiskBytes = drive.TotalSize;
                availableFreeBytes = drive.AvailableFreeSpace;
            }
        }
        catch
        {
            // ignore, keep nulls
        }

        long librarySizeBytes = 0;
        try
        {
            // Offload heavy IO to background thread
            librarySizeBytes = await Task.Run(() => CalculateDirectorySizeSafe(libraryPath));
        }
        catch
        {
            // ignore, keep zero
        }

        // Calculate estimated total library size when fully populated
        long estimatedTotalLibrarySizeBytes = 0;
        try
        {
            estimatedTotalLibrarySizeBytes = await Task.Run(() => CalculateEstimatedTotalLibrarySizeAsync(libraryPath, librarySizeBytes));
        }
        catch
        {
            // ignore, keep zero
        }

        return new StorageStats(totalDiskBytes, availableFreeBytes, librarySizeBytes, estimatedTotalLibrarySizeBytes);
    }

    private static long CalculateDirectorySizeSafe(string folder)
    {
        long size = 0;
        try
        {
            foreach (var file in Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    size += new FileInfo(file).Length;
                }
                catch
                {
                }
            }

            foreach (var sub in Directory.EnumerateDirectories(folder))
            {
                size += CalculateDirectorySizeSafe(sub);
            }
        }
        catch
        {
            // ignore permission errors and continue best-effort
        }

        return size;
    }

    private static async Task<long> CalculateEstimatedTotalLibrarySizeAsync(string libraryPath, long currentLibrarySizeBytes)
    {
        if (currentLibrarySizeBytes == 0)
            return 0;

        try
        {
            // Count total releases and releases with media files
            int totalReleases = 0;
            int releasesWithMedia = 0;

            foreach (var artistDir in Directory.EnumerateDirectories(libraryPath))
            {
                foreach (var releaseDir in Directory.EnumerateDirectories(artistDir))
                {
                    totalReleases++;
                    
                    // Check if this release has any audio files
                    var hasAudioFiles = Directory.EnumerateFiles(releaseDir, "*.*", SearchOption.TopDirectoryOnly)
                        .Any(file => IsAudioFile(file));
                    
                    if (hasAudioFiles)
                        releasesWithMedia++;
                }
            }

            // If no releases or all releases have media, return current size
            if (totalReleases == 0 || releasesWithMedia == totalReleases)
                return currentLibrarySizeBytes;

            // Calculate coverage percentage and estimate total size
            var coveragePercentage = (double)releasesWithMedia / totalReleases;
            if (coveragePercentage > 0)
            {
                var estimatedTotalSize = (long)(currentLibrarySizeBytes / coveragePercentage);
                return estimatedTotalSize;
            }

            return currentLibrarySizeBytes;
        }
        catch
        {
            // If we can't calculate, return current size
            return currentLibrarySizeBytes;
        }
    }

    private static bool IsAudioFile(string filePath)
    {
        var ext = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
        return ext is ".mp3" or ".flac" or ".wav" or ".m4a" or ".ogg" or ".aac";
    }
}

public record StorageStats(long? TotalDiskBytes, long? AvailableFreeBytes, long LibrarySizeBytes, long EstimatedTotalLibrarySizeBytes);