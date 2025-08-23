namespace MusicGQL.Features.External.Downloads.Sabnzbd.Configuration;

public class SabnzbdOptions
{
    public const string SectionName = "SABnzbd";

    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
    public string? Category { get; set; }
    public string? CompletedPath { get; set; }

    /// <summary>
    /// Optional: the Prowlarr base URL as seen from inside the Docker network.
    /// When SAB needs to fetch a URL hosted by Prowlarr (addurl), we will rewrite
    /// the external Prowlarr base to this internal base.
    /// Example: http://prowlarr:9696
    /// </summary>
    public string? BaseUrlToProwlarr { get; set; }
}


