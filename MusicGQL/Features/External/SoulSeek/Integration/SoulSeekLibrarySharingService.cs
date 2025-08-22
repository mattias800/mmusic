using MusicGQL.Features.ServerSettings;
using Soulseek;
using System.IO;
using System.Net;
using SystemIOPath = System.IO.Path;
using SystemIODirectory = System.IO.Directory;
using SystemIOFile = System.IO.File;
using SystemIOFileInfo = System.IO.FileInfo;

namespace MusicGQL.Features.External.SoulSeek.Integration;

/// <summary>
/// Service for sharing the music library on the Soulseek network
/// </summary>
public class SoulSeekLibrarySharingService(
    SoulseekClient client,
    ServerSettingsAccessor serverSettingsAccessor,
    ILogger<SoulSeekLibrarySharingService> logger
)
{
    private bool _isSharingEnabled = false;
    private int _listeningPort = 50300; // Default Soulseek port
    private string? _rootAlias;

    private static readonly string[] AudioExtensions = new[] { ".mp3", ".flac", ".m4a", ".wav", ".ogg", ".aac", ".wma", ".webm" };

    /// <summary>
    /// Gets the current sharing status
    /// </summary>
    public bool IsSharingEnabled => _isSharingEnabled;

    /// <summary>
    /// Gets the number of shared files
    /// </summary>
    public int SharedFileCount { get; private set; }

    /// <summary>
    /// Gets the listening port for incoming connections
    /// </summary>
    public int ListeningPort => _listeningPort;

    /// <summary>
    /// Configures the Soulseek client to enable incoming requests and wire share resolvers.
    /// Call this before connecting.
    /// </summary>
    public async Task ConfigureClientAsync()
    {
        try
        {
            var settings = await serverSettingsAccessor.GetAsync();

            if (!settings.SoulSeekLibrarySharingEnabled)
            {
                logger.LogInformation("[SoulSeek] Library sharing disabled; client not configured for shares");
                return;
            }

            _listeningPort = settings.SoulSeekListeningPort > 0 && settings.SoulSeekListeningPort <= 65535
                ? settings.SoulSeekListeningPort
                : 50300;

            // Precompute alias from configured library folder (folder name)
            if (!string.IsNullOrWhiteSpace(settings.LibraryPath))
            {
                _rootAlias = SystemIOPath.GetFileName(settings.LibraryPath.TrimEnd(SystemIOPath.DirectorySeparatorChar, SystemIOPath.AltDirectorySeparatorChar));
                if (string.IsNullOrWhiteSpace(_rootAlias)) _rootAlias = "library";
            }
            else
            {
                _rootAlias = "library";
            }

            var patch = new SoulseekClientOptionsPatch(
                listenIPAddress: IPAddress.Any,
                listenPort: _listeningPort,
                enableListener: true,
                browseResponseResolver: BrowseResponseResolver,
                directoryContentsResolver: DirectoryContentsResponseResolver,
                enqueueDownload: EnqueueDownload
            );

            await client.ReconfigureOptionsAsync(patch);

            logger.LogInformation("[SoulSeek] Client configured for sharing on port {Port}; alias={Alias}", _listeningPort, _rootAlias);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[SoulSeek] Failed to configure client for library sharing");
        }
    }

    /// <summary>
    /// Initializes the library sharing service
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            var settings = await serverSettingsAccessor.GetAsync();

            if (!settings.SoulSeekLibrarySharingEnabled)
            {
                logger.LogInformation("[SoulSeek] Library sharing is disabled in configuration");
                return;
            }

            await ConfigureClientAsync();

            logger.LogInformation("[SoulSeek] Using SDK {Version}", typeof(SoulseekClient).Assembly.GetName().Version?.ToString());
            logger.LogInformation("[SoulSeek] Library sharing service initialized");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[SoulSeek] Failed to initialize library sharing service");
        }
    }

    /// <summary>
    /// Starts sharing the music library
    /// </summary>
    public async Task StartSharingAsync()
    {
        try
        {
            var settings = await serverSettingsAccessor.GetAsync();

            if (!settings.SoulSeekLibrarySharingEnabled)
            {
                logger.LogInformation("[SoulSeek] Library sharing is disabled in configuration");
                return;
            }

            var libraryPath = settings.LibraryPath;

            if (string.IsNullOrWhiteSpace(libraryPath))
            {
                logger.LogWarning("[SoulSeek] Library path not configured, cannot start sharing");
                return;
            }

            if (!SystemIODirectory.Exists(libraryPath))
            {
                logger.LogWarning("[SoulSeek] Library path does not exist: {Path}", libraryPath);
                return;
            }

            logger.LogInformation("[SoulSeek] Sharing library path: {Path}", libraryPath);

            // Compute and publish share counts
            var (dirCount, fileCount) = await CountLibraryAsync(libraryPath);
            SharedFileCount = fileCount;

            try
            {
                await client.SetSharedCountsAsync(dirCount, fileCount);
                logger.LogInformation("[SoulSeek] Published share counts: {Dirs} directories, {Files} files", dirCount, fileCount);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[SoulSeek] Failed to publish shared counts; will continue");
            }

            _isSharingEnabled = true;

            logger.LogInformation(
                "[SoulSeek] Library sharing started successfully. Shared files from {Path}",
                libraryPath
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[SoulSeek] Failed to start library sharing");
            _isSharingEnabled = false;
        }
    }

    /// <summary>
    /// Stops sharing the music library
    /// </summary>
    public async Task StopSharingAsync()
    {
        try
        {
            _isSharingEnabled = false;

            // Inform the server that nothing is shared now (best-effort)
            try { await client.SetSharedCountsAsync(0, 0); } catch { }

            logger.LogInformation("[SoulSeek] Library sharing stopped");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[SoulSeek] Failed to stop library sharing");
        }
    }

    /// <summary>
    /// Refreshes the share index (useful after library changes)
    /// </summary>
    public async Task RefreshSharesAsync()
    {
        if (_isSharingEnabled)
        {
            logger.LogInformation("[SoulSeek] Refreshing share index...");
            await StartSharingAsync();
        }
    }

    /// <summary>
    /// Validates that the requested path is within the shared library directory
    /// </summary>
    private bool IsValidSharePath(string requestedPath)
    {
        try
        {
            var settings = serverSettingsAccessor.GetAsync().Result;
            var libraryPath = settings.LibraryPath;

            if (string.IsNullOrWhiteSpace(libraryPath))
                return false;

            var normalizedRequestedPath = SystemIOPath.GetFullPath(requestedPath);
            var normalizedLibraryPath = SystemIOPath.GetFullPath(libraryPath);

            // Ensure the requested path is within the library directory
            return normalizedRequestedPath.StartsWith(normalizedLibraryPath, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Soulseek callback: Build the full browse response (all directories/files)
    /// </summary>
    private async Task<BrowseResponse> BrowseResponseResolver(string username, IPEndPoint endpoint)
    {
        try
        {
            var directories = await BuildBrowseDirectoriesAsync();
            return new BrowseResponse(directories);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[SoulSeek] Failed to build browse response");
            return new BrowseResponse();
        }
    }

    /// <summary>
    /// Soulseek callback: Return the contents of a specific directory
    /// </summary>
    private async Task<IEnumerable<Soulseek.Directory>> DirectoryContentsResponseResolver(string username, IPEndPoint endpoint, int token, string directory)
    {
        try
        {
            var list = await BuildDirectoryListingAsync(directory);
            return new[] { list };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[SoulSeek] Failed to build directory contents for {Dir}", directory);
            return new[] { new Soulseek.Directory(directory) };
        }
    }

    /// <summary>
    /// Soulseek callback: Handle a remote download request by uploading the requested file
    /// </summary>
    private Task EnqueueDownload(string username, IPEndPoint endpoint, string filename)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var local = await MapRemoteToLocalAsync(filename);
                if (string.IsNullOrWhiteSpace(local) || !IsValidSharePath(local) || !SystemIOFile.Exists(local))
                {
                    logger.LogInformation("[SoulSeek] Rejecting upload for {User}: file not shared or not found: {File}", username, filename);
                    return;
                }

                var size = new SystemIOFileInfo(local).Length;

                await client.UploadAsync(
                    username,
                    filename, // remote path as requested
                    size: size,
                    inputStreamFactory: (startOffset) =>
                    {
                        var stream = new FileStream(local, FileMode.Open, FileAccess.Read, FileShare.Read);
                        if (startOffset > 0) stream.Seek(startOffset, SeekOrigin.Begin);
                        return Task.FromResult((Stream)stream);
                    },
                    options: new TransferOptions(
                        seekInputStreamAutomatically: false,
                        disposeInputStreamOnCompletion: true
                    )
                );

                logger.LogInformation("[SoulSeek] Completed upload to {User}: {File}", username, filename);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[SoulSeek] Upload failed for {User}: {File}", username, filename);
            }
        });

        return Task.CompletedTask;
    }

    /// <summary>
    /// Build a complete directory list for browse response
    /// </summary>
    private async Task<IEnumerable<Soulseek.Directory>> BuildBrowseDirectoriesAsync()
    {
        var settings = await serverSettingsAccessor.GetAsync();
        var libraryPath = settings.LibraryPath;

        if (string.IsNullOrWhiteSpace(libraryPath) || !SystemIODirectory.Exists(libraryPath))
        {
            return Array.Empty<Soulseek.Directory>();
        }

        var alias = _rootAlias ?? SystemIOPath.GetFileName(libraryPath.TrimEnd(SystemIOPath.DirectorySeparatorChar, SystemIOPath.AltDirectorySeparatorChar)) ?? "library";

        var directories = new Dictionary<string, Soulseek.Directory>(StringComparer.OrdinalIgnoreCase);

        // Seed all directories (including the root alias) so empty folders still appear
        directories[alias] = new Soulseek.Directory(alias);

        foreach (var dir in SystemIODirectory.EnumerateDirectories(libraryPath, "*", SearchOption.AllDirectories))
        {
            var remoteDir = ToRemotePath(alias, libraryPath, dir);
            if (!directories.ContainsKey(remoteDir))
            {
                directories[remoteDir] = new Soulseek.Directory(remoteDir);
            }
        }

        // Add files, grouped by directory
        var files = SystemIODirectory
            .EnumerateFiles(libraryPath, "*", SearchOption.AllDirectories)
            .Where(f => AudioExtensions.Contains(SystemIOPath.GetExtension(f).ToLowerInvariant()));

        var groups = files
            .GroupBy(f => ToRemotePath(alias, libraryPath, SystemIOPath.GetDirectoryName(f)!))
            .Select(g => new
            {
                Dir = g.Key,
                Files = g.Select(f => CreateSoulseekFile(alias, libraryPath, f)).OrderBy(sf => sf.Filename)
            });

        foreach (var g in groups)
        {
            directories[g.Dir] = new Soulseek.Directory(g.Dir, g.Files);
        }

        return directories.Values;
    }

    /// <summary>
    /// Build the listing for a single directory
    /// </summary>
    private async Task<Soulseek.Directory> BuildDirectoryListingAsync(string remoteDirectory)
    {
        var settings = await serverSettingsAccessor.GetAsync();
        var libraryPath = settings.LibraryPath;

        var alias = _rootAlias ?? SystemIOPath.GetFileName(libraryPath.TrimEnd(SystemIOPath.DirectorySeparatorChar, SystemIOPath.AltDirectorySeparatorChar)) ?? "library";
        var localDir = await MapRemoteDirectoryToLocalAsync(remoteDirectory);

        if (string.IsNullOrWhiteSpace(localDir) || !SystemIODirectory.Exists(localDir))
        {
            return new Soulseek.Directory(remoteDirectory);
        }

        var files = SystemIODirectory
            .EnumerateFiles(localDir, "*", SearchOption.TopDirectoryOnly)
            .Where(f => AudioExtensions.Contains(SystemIOPath.GetExtension(f).ToLowerInvariant()))
            .Select(f => CreateSoulseekFile(alias, libraryPath, f))
            .OrderBy(sf => sf.Filename);

        return new Soulseek.Directory(remoteDirectory, files);
    }

    private Soulseek.File CreateSoulseekFile(string alias, string libraryPath, string localFile)
    {
        var size = new SystemIOFileInfo(localFile).Length;
        var extension = SystemIOPath.GetExtension(localFile).TrimStart('.').ToLowerInvariant();
        var remote = ToRemotePath(alias, libraryPath, localFile);
        return new Soulseek.File(1, remote, size, extension, attributeList: null);
    }

    private static string ToRemotePath(string alias, string libraryPath, string localPath)
    {
        var fullLocal = SystemIOPath.GetFullPath(localPath);
        var fullRoot = SystemIOPath.GetFullPath(libraryPath.TrimEnd(SystemIOPath.DirectorySeparatorChar, SystemIOPath.AltDirectorySeparatorChar));

        string relative = string.Empty;
        if (fullLocal.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
        {
            relative = fullLocal.Length == fullRoot.Length
                ? string.Empty
                : fullLocal.Substring(fullRoot.Length + 1);
        }

        // Build Windows-style path for Soulseek
        var remote = string.IsNullOrEmpty(relative)
            ? alias
            : alias + "\\" + relative.Replace('/', '\\');

        return remote;
    }

    private async Task<string?> MapRemoteToLocalAsync(string remotePath)
    {
        var settings = await serverSettingsAccessor.GetAsync();
        return MapRemoteToLocal(settings.LibraryPath, remotePath);
    }

    private async Task<string?> MapRemoteDirectoryToLocalAsync(string remoteDirectory)
    {
        var settings = await serverSettingsAccessor.GetAsync();
        return MapRemoteToLocal(settings.LibraryPath, remoteDirectory);
    }

    private string? MapRemoteToLocal(string libraryPath, string remotePath)
    {
        if (string.IsNullOrWhiteSpace(libraryPath) || string.IsNullOrWhiteSpace(remotePath)) return null;

        var alias = _rootAlias ?? SystemIOPath.GetFileName(libraryPath.TrimEnd(SystemIOPath.DirectorySeparatorChar, SystemIOPath.AltDirectorySeparatorChar)) ?? "library";

        var normalizedRemote = remotePath.Replace('/', '\\');
        if (!normalizedRemote.StartsWith(alias + "\\") && !normalizedRemote.Equals(alias, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var sub = normalizedRemote.Equals(alias, StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : normalizedRemote.Substring(alias.Length + 1);

        var local = string.IsNullOrEmpty(sub) ? libraryPath : SystemIOPath.Combine(libraryPath, sub.Replace('\\', SystemIOPath.DirectorySeparatorChar));

        // Normalize and ensure within root
        try
        {
            var fullLocal = SystemIOPath.GetFullPath(local);
            var fullRoot = SystemIOPath.GetFullPath(libraryPath);
            if (!fullLocal.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase)) return null;
            return fullLocal;
        }
        catch
        {
            return null;
        }
    }

    private async Task<(int DirCount, int FileCount)> CountLibraryAsync(string libraryPath)
    {
        return await Task.Run(() =>
        {
            var dirs = 0;
            var files = 0;

            try
            {
                if (SystemIODirectory.Exists(libraryPath))
                {
                    dirs = 1; // root alias
                    foreach (var _ in SystemIODirectory.EnumerateDirectories(libraryPath, "*", SearchOption.AllDirectories)) dirs++;
                    files = SystemIODirectory.EnumerateFiles(libraryPath, "*", SearchOption.AllDirectories)
                        .Count(f => AudioExtensions.Contains(SystemIOPath.GetExtension(f).ToLowerInvariant()));
                }
            }
            catch
            {
                // ignore
            }

            return (dirs, files);
        });
    }

    /// <summary>
    /// Gets sharing statistics
    /// </summary>
    public async Task<SharingStatistics> GetSharingStatisticsAsync()
    {
        var settings = await serverSettingsAccessor.GetAsync();
        var libraryPath = settings.LibraryPath;

        if (string.IsNullOrWhiteSpace(libraryPath) || !SystemIODirectory.Exists(libraryPath))
        {
            return new SharingStatistics
            {
                IsSharingEnabled = false,
                SharedFileCount = 0,
                LibraryPath = libraryPath,
                ListeningPort = _listeningPort,
                TotalLibrarySize = 0
            };
        }

        var totalSize = await CalculateLibrarySizeAsync(libraryPath);
        var (_, fileCount) = await CountLibraryAsync(libraryPath);

        return new SharingStatistics
        {
            IsSharingEnabled = _isSharingEnabled,
            SharedFileCount = fileCount,
            LibraryPath = libraryPath,
            ListeningPort = _listeningPort,
            TotalLibrarySize = totalSize
        };
    }

    /// <summary>
    /// Calculates the total size of the music library
    /// </summary>
    private async Task<long> CalculateLibrarySizeAsync(string libraryPath)
    {
        try
        {
            return await Task.Run(() =>
            {
                var totalSize = 0L;

                foreach (var file in SystemIODirectory.EnumerateFiles(libraryPath, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        var extension = SystemIOPath.GetExtension(file).ToLowerInvariant();
                        if (AudioExtensions.Contains(extension))
                        {
                            var fileInfo = new SystemIOFileInfo(file);
                            totalSize += fileInfo.Length;
                        }
                    }
                    catch
                    {
                        // Ignore individual file errors
                    }
                }

                return totalSize;
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[SoulSeek] Failed to calculate library size");
            return 0;
        }
    }

    /// <summary>
    /// Disposes the service
    /// </summary>
    public async Task DisposeAsync()
    {
        try
        {
            await StopSharingAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[SoulSeek] Error during service disposal");
        }
    }
}

/// <summary>
/// Statistics about the current sharing status
/// </summary>
public class SharingStatistics
{
    public bool IsSharingEnabled { get; set; }
    public int SharedFileCount { get; set; }
    public string LibraryPath { get; set; } = string.Empty;
    public int ListeningPort { get; set; }
    public long TotalLibrarySize { get; set; }
}
