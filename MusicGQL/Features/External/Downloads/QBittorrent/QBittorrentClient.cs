using System.Net;
using System.Net.Http.Headers;
using MusicGQL.Features.External.Downloads.QBittorrent.Configuration;
using Microsoft.Extensions.Options;

namespace MusicGQL.Features.External.Downloads.QBittorrent;

public class QBittorrentClient(HttpClient httpClient, IOptions<QBittorrentOptions> options, ILogger<QBittorrentClient> logger)
{
    private string? cookie;

    private async Task<bool> EnsureLoginAsync(CancellationToken cancellationToken)
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl)) return false;
        if (!string.IsNullOrEmpty(cookie)) return true;
        try
        {
            var url = $"{baseUrl}/api/v2/auth/login";
            logger.LogDebug("[qBittorrent] POST {Url} (login)", url);
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("username", options.Value.Username ?? string.Empty),
                new KeyValuePair<string,string>("password", options.Value.Password ?? string.Empty),
            });
            var resp = await httpClient.PostAsync(url, content, cancellationToken);
            if (resp.IsSuccessStatusCode)
            {
                if (resp.Headers.TryGetValues("Set-Cookie", out var vals))
                {
                    cookie = vals.FirstOrDefault();
                    logger.LogInformation("[qBittorrent] Logged in successfully");
                    return true;
                }
            }
            else
            {
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("[qBittorrent] Login failed HTTP {Status}. Body: {Body}", (int)resp.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "qBittorrent login failed");
        }
        return false;
    }

    public async Task<bool> AddMagnetAsync(string magnetUrl, string? savePath, CancellationToken cancellationToken)
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl)) return false;
        if (!await EnsureLoginAsync(cancellationToken)) return false;
        try
        {
            var url = $"{baseUrl}/api/v2/torrents/add";
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            if (!string.IsNullOrWhiteSpace(cookie)) req.Headers.Add("Cookie", cookie);
            var form = new MultipartFormDataContent
            {
                { new StringContent(magnetUrl), "urls" }
            };
            if (!string.IsNullOrWhiteSpace(savePath))
            {
                form.Add(new StringContent(savePath), "savepath");
            }
            req.Content = form;
            logger.LogDebug("[qBittorrent] POST {Url} (add magnet)", url);
            var resp = await httpClient.SendAsync(req, cancellationToken);
            var ok = resp.IsSuccessStatusCode;
            if (!ok)
            {
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("[qBittorrent] Add magnet failed HTTP {Status}. Body: {Body}", (int)resp.StatusCode, body);
            }
            return ok;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "qBittorrent add magnet failed");
            return false;
        }
    }
}


