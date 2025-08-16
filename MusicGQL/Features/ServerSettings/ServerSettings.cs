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
        if (string.IsNullOrWhiteSpace(settings.LibraryPath))
        {
            return null;
        }

        try
        {
            var directory = new DirectoryInfo(settings.LibraryPath);
            if (!directory.Exists)
            {
                return null;
            }

            var totalSize = GetDirectorySize(directory);
            var totalFiles = GetFileCount(directory);

            return new StorageStats
            {
                TotalSizeBytes = totalSize,
                TotalFiles = totalFiles,
                LibraryPath = settings.LibraryPath
            };
        }
        catch
        {
            return null;
        }
    }

    private static long GetDirectorySize(DirectoryInfo directory)
    {
        long size = 0;
        try
        {
            var files = directory.GetFiles();
            foreach (var file in files)
            {
                size += file.Length;
            }

            var subdirectories = directory.GetDirectories();
            foreach (var subdirectory in subdirectories)
            {
                size += GetDirectorySize(subdirectory);
            }
        }
        catch
        {
            // Ignore errors and return current size
        }

        return size;
    }

    private static int GetFileCount(DirectoryInfo directory)
    {
        int count = 0;
        try
        {
            count += directory.GetFiles().Length;

            var subdirectories = directory.GetDirectories();
            foreach (var subdirectory in subdirectories)
            {
                count += GetFileCount(subdirectory);
            }
        }
        catch
        {
            // Ignore errors and return current count
        }

        return count;
    }
}

public record StorageStats
{
    public long TotalSizeBytes { get; init; }
    public int TotalFiles { get; init; }
    public string LibraryPath { get; init; } = string.Empty;

    public string TotalSizeFormatted()
    {
        var sizes = new[] { "B", "KB", "MB", "GB", "TB" };
        var order = 0;
        var size = (double)TotalSizeBytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }
}