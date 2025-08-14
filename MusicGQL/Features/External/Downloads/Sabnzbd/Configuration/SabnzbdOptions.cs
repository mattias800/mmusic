namespace MusicGQL.Features.External.Downloads.Sabnzbd.Configuration;

public class SabnzbdOptions
{
    public const string SectionName = "SABnzbd";

    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
    public string? Category { get; set; }
    public string? CompletedPath { get; set; }
}


