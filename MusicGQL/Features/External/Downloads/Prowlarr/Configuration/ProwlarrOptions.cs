namespace MusicGQL.Features.External.Downloads.Prowlarr.Configuration;

public class ProwlarrOptions
{
    public const string SectionName = "Prowlarr";

    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
    
    /// <summary>
    /// Optional: the SABnzbd base URL as seen from inside the Docker network.
    /// If we ever need to provide SAB URLs to Prowlarr or construct SAB-facing URLs
    /// for container consumption, we can rewrite to this base.
    /// Example: http://sabnzbd:6789
    /// </summary>
    public string? BaseUrlToSabnzbd { get; set; }
    
    /// <summary>
    /// Optional list of indexer IDs to restrict searches to (matches Prowlarr indexer IDs).
    /// If provided, the client will repeat indexers=ID for each value in API calls.
    /// </summary>
    public int[]? IndexerIds { get; set; }
    
    /// <summary>
    /// Timeout in seconds for Prowlarr API requests. Default is 30 seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Maximum number of retry attempts for failed requests. Default is 2.
    /// </summary>
    public int MaxRetries { get; set; } = 2;
    
    /// <summary>
    /// Whether to test connectivity before attempting searches. Default is true.
    /// </summary>
    public bool TestConnectivityFirst { get; set; } = true;
    
    /// <summary>
    /// Delay in seconds between retry attempts. Default is 2 seconds. Note: a minimum of 2s is enforced in code.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 2;
    
    /// <summary>
    /// Whether to enable detailed logging for debugging. Default is false.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;
    
    /// <summary>
    /// Maximum number of concurrent requests to Prowlarr. Default is 1.
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 1;
}

