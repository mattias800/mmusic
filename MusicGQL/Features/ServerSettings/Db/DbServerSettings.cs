namespace MusicGQL.Features.ServerSettings.Db;

public class DbServerSettings
{
    public int Id { get; set; }

    public string LibraryPath { get; set; } = string.Empty;
    public string DownloadPath { get; set; } = string.Empty;

    // Root folder for application logs (e.g., /Logs). May be empty/blank to disable file logging.
    public string? LogsFolderPath { get; set; } = string.Empty;

    // Max time (seconds) a SoulSeek search may run before yielding to next queued job.
    // When queue has more items, an ongoing search exceeding this will be aborted and re-queued to the back.
    public int SoulSeekSearchTimeLimitSeconds { get; set; } = 60;

    // Max seconds to wait without receiving data during a SoulSeek transfer before cancelling it
    public int SoulSeekNoDataTimeoutSeconds { get; set; } = 20;

    // Max seconds to wait in a remote user's upload queue before cancelling (no data yet)
    // Default increased to 15 minutes as some users may have very long queues
    public int SoulSeekQueueWaitTimeoutSeconds { get; set; } = 900;

    // Enable batch downloading from Soulseek users (discover additional releases after successful downloads)
    public bool SoulSeekBatchDownloadingEnabled { get; set; } = true;

    // Maximum number of additional releases to discover from a single user (prevents queue overflow)
    public int SoulSeekMaxReleasesPerUserDiscovery { get; set; } = 5;

    // Enable search query enhancement for short release titles (adds contextual keywords)
    public bool SearchEnhanceShortTitles { get; set; } = true;

    // Enable sharing the music library on Soulseek network
    public bool SoulSeekLibrarySharingEnabled { get; set; } = true;

    // Soulseek listening port for incoming connections (0 = auto-detect)
    public int SoulSeekListeningPort { get; set; } = 0;

    // Number of concurrent download slots to use
    public int DownloadSlotCount { get; set; } = 3;

    // Public base URL for building absolute links (e.g., https://mmusic.com)
    public string PublicBaseUrl { get; set; } = string.Empty;

    // Top Tracks Service Configuration
    public bool ListenBrainzTopTracksEnabled { get; set; } = true; // Primary source
    public bool SpotifyTopTracksEnabled { get; set; } = false; // Disabled by default
    public bool LastFmTopTracksEnabled { get; set; } = false; // Disabled by default

    // SoulSeek connection (non-secret settings)
    public string SoulSeekHost { get; set; } = "vps.slsknet.org";
    public int SoulSeekPort { get; set; } = 2271;
    public string SoulSeekUsername { get; set; } = string.Empty;

    // Prowlarr (non-secret settings; API key should come from env/secret store)
    public string? ProwlarrBaseUrl { get; set; }
    public int ProwlarrTimeoutSeconds { get; set; } = 30;
    public int ProwlarrMaxRetries { get; set; } = 2;
    public int ProwlarrRetryDelaySeconds { get; set; } = 1;
    public bool ProwlarrTestConnectivityFirst { get; set; } = true;
    public bool ProwlarrEnableDetailedLogging { get; set; } = false;
    public int ProwlarrMaxConcurrentRequests { get; set; } = 1;

    // qBittorrent (non-secret settings; password should come from env/secret store)
    public string? QBittorrentBaseUrl { get; set; }
    public string? QBittorrentUsername { get; set; }
    public string? QBittorrentSavePath { get; set; }

    // Downloader enable toggles
    public bool EnableSabnzbdDownloader { get; set; } = true;
    public bool EnableQBittorrentDownloader { get; set; } = true;
    public bool EnableSoulSeekDownloader { get; set; } = true;

    // Discography handling
    public bool DiscographyEnabled { get; set; } = true;
    public string? DiscographyStagingPath { get; set; } = string.Empty;
}

public static class DefaultDbServerSettingsProvider
{
    public const int ServerSettingsSingletonId = 10;

    public static DbServerSettings GetDefault()
    {
        return new()
        {
            Id = ServerSettingsSingletonId,
            LibraryPath = "",
            DownloadPath = "",
            LogsFolderPath = "",
            SoulSeekSearchTimeLimitSeconds = 60,
            SoulSeekNoDataTimeoutSeconds = 20,
            SoulSeekQueueWaitTimeoutSeconds = 900,
            SoulSeekBatchDownloadingEnabled = true,
            SoulSeekMaxReleasesPerUserDiscovery = 5,
            SearchEnhanceShortTitles = true,
            SoulSeekLibrarySharingEnabled = true,
            SoulSeekListeningPort = 50300,
            DownloadSlotCount = 3,
            PublicBaseUrl = "",
            ListenBrainzTopTracksEnabled = true,
            SpotifyTopTracksEnabled = false,
            LastFmTopTracksEnabled = false,
            SoulSeekHost = "vps.slsknet.org",
            SoulSeekPort = 2271,
            SoulSeekUsername = "",
            ProwlarrBaseUrl = null,
            ProwlarrTimeoutSeconds = 30,
            ProwlarrMaxRetries = 2,
            ProwlarrRetryDelaySeconds = 1,
            ProwlarrTestConnectivityFirst = true,
            ProwlarrEnableDetailedLogging = false,
            ProwlarrMaxConcurrentRequests = 1,
            QBittorrentBaseUrl = null,
            QBittorrentUsername = null,
            QBittorrentSavePath = null,
            EnableSabnzbdDownloader = true,
            EnableQBittorrentDownloader = true,
            EnableSoulSeekDownloader = true,
            DiscographyEnabled = true,
            DiscographyStagingPath = "",
        };
    }
}
