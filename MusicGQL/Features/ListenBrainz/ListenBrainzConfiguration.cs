namespace MusicGQL.Features.ListenBrainz;

public class ListenBrainzConfiguration
{
    public const string SectionName = "ListenBrainz";
    
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.listenbrainz.org";
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 1;

    // Similar artists (Labs) specific settings
    // Default algorithm known to return results broadly
    public string SimilarArtistsAlgorithm { get; set; } = "session_based_days_9000_session_300_contribution_5_threshold_15_limit_50_skip_30";
    public string SimilarArtistsEndpoint { get; set; } = "https://labs.api.listenbrainz.org/similar-artists/json";
    
    // Popularity API specific settings
    public int PopularityTimeoutSeconds { get; set; } = 15;
    public int PopularityMaxRetries { get; set; } = 2;
    public int PopularityRetryDelaySeconds { get; set; } = 1;
}
