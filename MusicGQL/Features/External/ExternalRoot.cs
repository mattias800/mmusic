using Microsoft.Extensions.Options;
using MusicGQL;
using MusicGQL.Features.Downloads.Services;
using MusicGQL.Features.External.Downloads.Sabnzbd.Configuration;
using MusicGQL.Features.External.SoulSeek;
using MusicGQL.Features.ServerSettings;
using MusicGQL.Integration.ListenBrainz;
using MusicGQL.Integration.Spotify.Configuration;
using MusicGQL.Integration.Youtube.Configuration;
using SpotifyAPI.Web;

namespace MusicGQL.Features.External;

public record ExternalRoot
{
    [ID]
    public string Id => "External";

    public SoulSeekRoot SoulSeek() => new();

    [GraphQLName("testSabnzbdConnectivity")]
    public async Task<ConnectivityStatus> TestSabnzbdConnectivity(
        [Service] MusicGQL.Features.External.Downloads.Sabnzbd.SabnzbdClient client
    )
    {
        var (ok, msg) = await client.TestConnectivityAsync(CancellationToken.None);
        return new ConnectivityStatus(ok, msg);
    }

    [GraphQLName("testProwlarrConnectivity")]
    public async Task<ConnectivityStatus> TestProwlarrConnectivity(
        [Service] MusicGQL.Features.External.Downloads.Prowlarr.IProwlarrClient client
    )
    {
        var (ok, msg) = await client.TestConnectivityAsyncPublic(CancellationToken.None);
        return new ConnectivityStatus(ok, msg);
    }

    [GraphQLName("testListenBrainzConnectivity")]
    public async Task<ConnectivityStatus> TestListenBrainzConnectivity(
        [Service] ListenBrainzService listenBrainzService,
        [Service] ServerSettingsAccessor serverSettingsAccessor,
        [Service] IOptions<ListenBrainz.ListenBrainzConfiguration> listenBrainzOptions
    )
    {
        var apiKey = listenBrainzOptions.Value.ApiKey;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new ConnectivityStatus(false, "API key not configured");
        }

        var (isValid, user, message) = await listenBrainzService.ValidateTokenAsync(apiKey);
        if (isValid)
        {
            return new ConnectivityStatus(
                true,
                string.IsNullOrWhiteSpace(user) ? "ok" : $"ok (user: {user})"
            );
        }
        return new ConnectivityStatus(false, message ?? "invalid");
    }

    [GraphQLName("testYouTubeConnectivity")]
    public ConnectivityStatus TestYouTubeConnectivity(
        [Service] IOptions<YouTubeServiceOptions> options
    )
    {
        var hasKey = !string.IsNullOrWhiteSpace(options.Value.ApiKey);
        return new ConnectivityStatus(
            hasKey,
            hasKey ? "API key present" : "API key not configured"
        );
    }

    [GraphQLName("testSpotifyConnectivity")]
    public ConnectivityStatus TestSpotifyConnectivity(
        [Service] IOptions<SpotifyClientOptions> options
    )
    {
        var ok =
            !string.IsNullOrWhiteSpace(options.Value.ClientId)
            && !string.IsNullOrWhiteSpace(options.Value.ClientSecret);
        return new ConnectivityStatus(
            ok,
            ok ? "Client credentials present" : "ClientId/ClientSecret not configured"
        );
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
        [Service] MusicGQL.Features.External.Downloads.QBittorrent.QBittorrentClient client
    )
    {
        var (ok, msg) = await client.TestConnectivityAsync(CancellationToken.None);
        return new ConnectivityStatus(ok, msg);
    }
}

public record ConnectivityStatus(bool Ok, string Message);
