using MusicGQL.Features.External.SoulSeek;

namespace MusicGQL.Features.External;

public record ExternalRoot
{
    [ID]
    public string Id => "External";

    public SoulSeekRoot SoulSeek() => new();

    [GraphQLName("testProwlarrConnectivity")]
    public async Task<ConnectivityStatus> TestProwlarrConnectivity(
        [Service] Microsoft.Extensions.Options.IOptions<MusicGQL.Features.External.Downloads.Prowlarr.Configuration.ProwlarrOptions> options,
        [Service] IHttpClientFactory httpClientFactory
    )
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
            return new ConnectivityStatus(false, "BaseUrl not configured");

        try
        {
            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(Math.Max(2, options.Value.TimeoutSeconds));
            using var resp = await client.GetAsync(baseUrl);
            return new ConnectivityStatus(resp.IsSuccessStatusCode, $"HTTP {(int)resp.StatusCode}");
        }
        catch (Exception ex)
        {
            return new ConnectivityStatus(false, ex.Message);
        }
    }

    [GraphQLName("testQBittorrentConnectivity")]
    public async Task<ConnectivityStatus> TestQBittorrentConnectivity(
        [Service] Microsoft.Extensions.Options.IOptions<MusicGQL.Features.External.Downloads.QBittorrent.Configuration.QBittorrentOptions> options,
        [Service] IHttpClientFactory httpClientFactory
    )
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
            return new ConnectivityStatus(false, "BaseUrl not configured");

        var url = baseUrl + "/api/v2/app/webapiVersion";
        try
        {
            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            using var resp = await client.GetAsync(url);
            return new ConnectivityStatus(resp.IsSuccessStatusCode, $"HTTP {(int)resp.StatusCode}");
        }
        catch (Exception ex)
        {
            return new ConnectivityStatus(false, ex.Message);
        }
    }
}

public record ConnectivityStatus(bool Ok, string Message);
