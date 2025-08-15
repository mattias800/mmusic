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
}


