using System.Net;
using System.Net.Http.Headers;
using MusicGQL.Features.External.Downloads.QBittorrent.Configuration;
using Microsoft.Extensions.Options;

namespace MusicGQL.Features.External.Downloads.QBittorrent;

public class QBittorrentClient(HttpClient httpClient, IOptions<QBittorrentOptions> options, ILogger<QBittorrentClient> logger, MusicGQL.Features.Downloads.Services.DownloadLogPathProvider logPathProvider)
{
    private string? cookie;
    private MusicGQL.Features.Downloads.Services.DownloadLogger? serviceLogger;

    private async Task<bool> EnsureLoginAsync(CancellationToken cancellationToken)
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl)) return false;
        if (!string.IsNullOrEmpty(cookie)) return true;
        try
        {
            try
            {
                var path = await logPathProvider.GetServiceLogFilePathAsync("qbittorrent", cancellationToken);
                if (!string.IsNullOrWhiteSpace(path) && serviceLogger == null)
                {
                    serviceLogger = new MusicGQL.Features.Downloads.Services.DownloadLogger(path!);
                }
            }
            catch { }
            var url = $"{baseUrl}/api/v2/auth/login";
            logger.LogDebug("[qBittorrent] POST {Url} (login)", url);
            try { serviceLogger?.Info($"POST {url} (login)"); } catch { }
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
                    try { serviceLogger?.Info("Logged in successfully"); } catch { }
                    return true;
                }
            }
            else
            {
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("[qBittorrent] Login failed HTTP {Status}. Body: {Body}", (int)resp.StatusCode, body);
                try { serviceLogger?.Warn($"Login failed HTTP {(int)resp.StatusCode}. Body: {body}"); } catch { }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "qBittorrent login failed");
            try { serviceLogger?.Error($"Login exception: {ex.Message}"); } catch { }
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
            try { serviceLogger?.Info($"POST {url} (add magnet)"); } catch { }
            var resp = await httpClient.SendAsync(req, cancellationToken);
            var ok = resp.IsSuccessStatusCode;
            if (!ok)
            {
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("[qBittorrent] Add magnet failed HTTP {Status}. Body: {Body}", (int)resp.StatusCode, body);
                try { serviceLogger?.Warn($"Add magnet failed HTTP {(int)resp.StatusCode}. Body: {body}"); } catch { }
            }
            return ok;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "qBittorrent add magnet failed");
            try { serviceLogger?.Error($"Add magnet exception: {ex.Message}"); } catch { }
            return false;
        }
    }

    public async Task<bool> AddByUrlAsync(string torrentUrl, string? savePath, CancellationToken cancellationToken)
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
                { new StringContent(torrentUrl), "urls" }
            };
            if (!string.IsNullOrWhiteSpace(savePath))
            {
                form.Add(new StringContent(savePath), "savepath");
            }
            req.Content = form;
            logger.LogDebug("[qBittorrent] POST {Url} (add by url)", url);
            try { serviceLogger?.Info($"POST {url} (add by url)"); } catch { }
            var resp = await httpClient.SendAsync(req, cancellationToken);
            var ok = resp.IsSuccessStatusCode;
            if (!ok)
            {
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("[qBittorrent] Add by url failed HTTP {Status}. Body: {Body}", (int)resp.StatusCode, body);
                try { serviceLogger?.Warn($"Add by url failed HTTP {(int)resp.StatusCode}. Body: {body}"); } catch { }
            }
            return ok;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "qBittorrent add by url failed");
            try { serviceLogger?.Error($"Add by url exception: {ex.Message}"); } catch { }
            return false;
        }
    }

    public async Task<(bool ok, string message)> TestConnectivityAsync(CancellationToken cancellationToken)
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl)) return (false, "BaseUrl not configured");
        try
        {
            if (serviceLogger == null)
            {
                var path = await logPathProvider.GetServiceLogFilePathAsync("qbittorrent", cancellationToken);
                if (!string.IsNullOrWhiteSpace(path)) serviceLogger = new MusicGQL.Features.Downloads.Services.DownloadLogger(path!);
            }
        }
        catch { }

        var url = baseUrl + "/api/v2/app/webapiVersion";
        try
        {
            try { serviceLogger?.Info($"Test connectivity: GET {url}"); } catch { }
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            var resp = await httpClient.SendAsync(req, cancellationToken);
            var msg = $"HTTP {(int)resp.StatusCode}";
            try { serviceLogger?.Info($"Result: {msg}"); } catch { }
            return (resp.IsSuccessStatusCode, msg);
        }
        catch (Exception ex)
        {
            try { serviceLogger?.Error($"Exception: {ex.Message}"); } catch { }
            return (false, ex.Message);
        }
    }
}


