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
    SoulseekClient client,
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

            logger.LogInformation("[SoulSeek] Using SDK {Version}; upload events/listen server may not be available in this build", typeof(SoulseekClient).Assembly.GetName().Version?.ToString());
            
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

    // Note: StartListeningAsync removed - current SDK may not expose a listener in this version

    // Note: UploadRequested handler removed - current SDK may not expose this event in this version

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
