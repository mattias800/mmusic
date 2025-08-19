using MusicGQL.Features.External.SoulSeek;
using MusicGQL.Integration.ListenBrainz;
using MusicGQL.Features.ServerSettings;
using Microsoft.Extensions.Options;
using MusicGQL.Integration.Youtube.Configuration;
using MusicGQL.Integration.Spotify.Configuration;
using SpotifyAPI.Web;
using MusicGQL;

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

    [GraphQLName("testListenBrainzConnectivity")]
    public async Task<ConnectivityStatus> TestListenBrainzConnectivity(
        [Service] ListenBrainzService listenBrainzService,
        [Service] ServerSettingsAccessor serverSettingsAccessor
    )
    {
        var s = await serverSettingsAccessor.GetAsync();
        if (string.IsNullOrWhiteSpace(s.ListenBrainzApiKey))
        {
            return new ConnectivityStatus(false, "API key not configured");
        }

        var (isValid, user, message) = await listenBrainzService.ValidateTokenAsync(s.ListenBrainzApiKey);
        if (isValid)
        {
            return new ConnectivityStatus(true, string.IsNullOrWhiteSpace(user) ? "ok" : $"ok (user: {user})");
        }
        return new ConnectivityStatus(false, message ?? "invalid");
    }

    [GraphQLName("testYouTubeConnectivity")]
    public ConnectivityStatus TestYouTubeConnectivity([Service] IOptions<YouTubeServiceOptions> options)
    {
        var hasKey = !string.IsNullOrWhiteSpace(options.Value.ApiKey);
        return new ConnectivityStatus(hasKey, hasKey ? "API key present" : "API key not configured");
    }

    [GraphQLName("testSpotifyConnectivity")]
    public ConnectivityStatus TestSpotifyConnectivity([Service] IOptions<SpotifyClientOptions> options)
    {
        var ok = !string.IsNullOrWhiteSpace(options.Value.ClientId) && !string.IsNullOrWhiteSpace(options.Value.ClientSecret);
        return new ConnectivityStatus(ok, ok ? "Client credentials present" : "ClientId/ClientSecret not configured");
    }

    [GraphQLName("testLastfmConnectivity")]
    public ConnectivityStatus TestLastfmConnectivity([Service] IOptions<LastfmOptions> options)
    {
        var ok = !string.IsNullOrWhiteSpace(options.Value.ApiKey);
        return new ConnectivityStatus(ok, ok ? "API key present" : "API key not configured");
    }

    [GraphQLName("testFanartConnectivity")]
    public ConnectivityStatus TestFanartConnectivity([Service] IOptions<FanartOptions> options)
    {
        var ok = !string.IsNullOrWhiteSpace(options.Value.ApiKey);
        return new ConnectivityStatus(ok, ok ? "API key present" : "Missing API key");
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
