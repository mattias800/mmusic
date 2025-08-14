namespace MusicGQL.Features.External.Downloads.QBittorrent.Configuration;

public class QBittorrentOptions
{
    public const string SectionName = "QBittorrent";

    public string? BaseUrl { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? SavePath { get; set; }
}


