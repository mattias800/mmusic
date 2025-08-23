namespace MusicGQL.Features.External.Downloads.Prowlarr.Configuration;

public class ProwlarrOptions
{
    public const string SectionName = "Prowlarr";

    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
    
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


