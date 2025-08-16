namespace MusicGQL.Features.ListenBrainz;

public class ListenBrainzConfiguration
{
    public const string SectionName = "ListenBrainz";
    
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.listenbrainz.org";
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 1;
}
