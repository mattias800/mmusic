namespace MusicGQL.Features.External.Downloads.Prowlarr.Configuration;

public class ProwlarrOptions
{
    public const string SectionName = "Prowlarr";

    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
}


