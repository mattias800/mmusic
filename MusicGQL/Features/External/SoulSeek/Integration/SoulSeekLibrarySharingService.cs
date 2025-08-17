using MusicGQL.Features.ServerSettings;
using Soulseek;
using System.IO;
using SystemIOPath = System.IO.Path;
using SystemIODirectory = System.IO.Directory;
using SystemIOFile = System.IO.File;
using SystemIOFileInfo = System.IO.FileInfo;

namespace MusicGQL.Features.External.SoulSeek.Integration;

/// <summary>
/// Service for sharing the music library on the Soulseek network
/// </summary>
public class SoulSeekLibrarySharingService(
    ISoulseekClient client,
    ServerSettingsAccessor serverSettingsAccessor,
    ILogger<SoulSeekLibrarySharingService> logger
)
{
    private object? _currentShareIndex; // TODO: Use correct type when identified
    private bool _isSharingEnabled = false;
    private int _listeningPort = 50300; // Default Soulseek port

    /// <summary>
    /// Gets the current sharing status
    /// </summary>
    public bool IsSharingEnabled => _isSharingEnabled;

    /// <summary>
    /// Gets the number of shared files
    /// </summary>
    public int SharedFileCount => 0; // TODO: Return actual count when share index is implemented

    /// <summary>
    /// Gets the listening port for incoming connections
    /// </summary>
    public int ListeningPort => _listeningPort;

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

            // TODO: Set up upload request handler when correct event types are identified
            // client.UploadRequested += OnUploadRequested;
            
            // TODO: Start listening for incoming connections when ListenAsync is available
            // await StartListeningAsync();
            
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

            logger.LogInformation("[SoulSeek] Building share index for library: {Path}", libraryPath);

            // TODO: Implement share index building when correct types are identified
            // var sharedRoots = new[] { libraryPath };
            // _currentShareIndex = await ShareIndexer.BuildAsync(sharedRoots);
            // await client.Shares.SetAsync(_currentShareIndex);
            
            logger.LogWarning("[SoulSeek] Share index building not yet implemented - waiting for correct Soulseek.NET types");

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
            if (_currentShareIndex != null)
            {
                // TODO: Clear shares when implemented
                // await client.Shares.SetAsync(new List<SharedFile>());
                _currentShareIndex = null;
            }

            _isSharingEnabled = false;

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
            await StopSharingAsync();
            await StartSharingAsync();
        }
    }

    /// <summary>
    /// Starts listening for incoming connections
    /// </summary>
    /// <remarks>
    /// TODO: Implement when ListenAsync method is available on Soulseek client
    /// </remarks>
    /*
    private async Task StartListeningAsync()
    {
        try
        {
            var settings = await serverSettingsAccessor.GetAsync();
            var configuredPort = settings.SoulSeekListeningPort;
            
            // Use configured port if specified, otherwise use default
            var startPort = configuredPort > 0 ? configuredPort : _listeningPort;
            var maxAttempts = configuredPort > 0 ? 1 : 10; // Only try configured port once

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    var port = startPort + attempt;
                    await client.ListenAsync(port);
                    _listeningPort = port;
                    logger.LogInformation("[SoulSeek] Started listening on port {Port}", port);
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "[SoulSeek] Failed to listen on port {Port}, trying next port", startPort + attempt);
                }
            }

            logger.LogWarning("[SoulSeek] Could not find an available port for listening");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[SoulSeek] Failed to start listening for incoming connections");
        }
    }
    */

    /// <summary>
    /// Handles incoming upload requests from other users
    /// </summary>
    /// <remarks>
    /// TODO: Implement when correct Soulseek.NET event types are identified
    /// </remarks>
    /*
    private void OnUploadRequested(object? sender, UploadRequestedEventArgs e)
    {
        try
        {
            logger.LogInformation(
                "[SoulSeek] Upload requested by {User} for file: {File} (Size: {Size} bytes)",
                e.Username,
                e.Filename,
                e.Size
            );

            // Validate the requested file path
            if (!IsValidSharePath(e.FullPath))
            {
                logger.LogWarning("[SoulSeek] Invalid share path requested: {Path}", e.FullPath);
                e.Supply(() => Stream.Null); // Supply empty stream for invalid requests
                return;
            }

            // Check if file exists
            if (!File.Exists(e.FullPath))
            {
                logger.LogWarning("[SoulSeek] Requested file does not exist: {Path}", e.FullPath);
                e.Supply(() => Stream.Null);
                return;
            }

            // Supply the file stream
            e.Supply(() => File.OpenRead(e.FullPath));

            logger.LogDebug("[SoulSeek] Successfully supplied file stream for: {File}", e.Filename);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[SoulSeek] Error handling upload request for file: {File}", e.Filename);
            
            // Supply empty stream on error
            try
            {
                e.Supply(() => Stream.Null);
            }
            catch
            {
                // Ignore errors in error handling
            }
        }
    }
    */

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

        return new SharingStatistics
        {
            IsSharingEnabled = _isSharingEnabled,
            SharedFileCount = 0, // TODO: Return actual count when share index is implemented
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
                var audioExtensions = new[] { ".mp3", ".flac", ".m4a", ".wav", ".ogg" };

                foreach (var file in SystemIODirectory.EnumerateFiles(libraryPath, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        var extension = SystemIOPath.GetExtension(file).ToLowerInvariant();
                        if (audioExtensions.Contains(extension))
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
            // TODO: Unsubscribe from upload events when implemented
            // client.UploadRequested -= OnUploadRequested;
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
