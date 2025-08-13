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

        return new StorageStats(totalDiskBytes, availableFreeBytes, librarySizeBytes);
    }

    [GraphQLName("hasLibraryManifest")]
    public async Task<bool> GetHasLibraryManifest([Service] ServerSettingsAccessor serverSettingsAccessor,
        [Service] LibraryManifestService manifestService)
    {
        var settings = await serverSettingsAccessor.GetAsync();
        return await manifestService.HasManifestAsync(settings.LibraryPath);
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
}

public record StorageStats(long? TotalDiskBytes, long? AvailableFreeBytes, long LibrarySizeBytes);