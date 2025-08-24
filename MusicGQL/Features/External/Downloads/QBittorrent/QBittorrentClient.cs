using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using MusicGQL.Features.External.Downloads.QBittorrent.Configuration;

namespace MusicGQL.Features.External.Downloads.QBittorrent;

public class QBittorrentClient(
    HttpClient httpClient,
    IOptions<QBittorrentOptions> options,
    ILogger<QBittorrentClient> logger,
    MusicGQL.Features.Downloads.Services.DownloadLogPathProvider logPathProvider
)
{
    public record TorrentInfo(
        string Hash,
        string Name,
        double Progress,
        string? State,
        string? SavePath,
        string? ContentPath
    );

    private string? cookie;
    private MusicGQL.Features.Downloads.Services.DownloadLogger? serviceLogger;

    public async Task<MusicGQL.Features.Downloads.Services.DownloadLogger> GetLogger()
    {
        if (serviceLogger == null)
        {
            var path = await logPathProvider.GetServiceLogFilePathAsync(
                "qbittorrent",
                CancellationToken.None
            );
            serviceLogger = new MusicGQL.Features.Downloads.Services.DownloadLogger(path);
        }
        return serviceLogger;
    }

    private async Task<bool> EnsureLoginAsync(CancellationToken cancellationToken)
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
            return false;
        if (!string.IsNullOrEmpty(cookie))
            return true;
        var serviceLogger = await GetLogger();
        try
        {
            var url = $"{baseUrl}/api/v2/auth/login";
            logger.LogDebug("[qBittorrent] POST {Url} (login)", url);
            serviceLogger.Info($"POST {url} (login)");
            var content = new FormUrlEncodedContent(
                new[]
                {
                    new KeyValuePair<string, string>(
                        "username",
                        options.Value.Username ?? string.Empty
                    ),
                    new KeyValuePair<string, string>(
                        "password",
                        options.Value.Password ?? string.Empty
                    ),
                }
            );
            var resp = await httpClient.PostAsync(url, content, cancellationToken);
            if (resp.IsSuccessStatusCode)
            {
                if (resp.Headers.TryGetValues("Set-Cookie", out var vals))
                {
                    cookie = vals.FirstOrDefault();
                    logger.LogInformation("[qBittorrent] Logged in successfully");
                    serviceLogger.Info("Logged in successfully");
                    return true;
                }
            }
            else
            {
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning(
                    "[qBittorrent] Login failed HTTP {Status}. Body: {Body}",
                    (int)resp.StatusCode,
                    body
                );
                serviceLogger.Warn($"Login failed HTTP {(int)resp.StatusCode}. Body: {body}");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "qBittorrent login failed");
            serviceLogger.Error($"Login exception: {ex.Message}");
        }
        return false;
    }

    public async Task<bool> AddMagnetAsync(
        string magnetUrl,
        string? savePath,
        CancellationToken cancellationToken
    )
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
            return false;
        var serviceLogger = await GetLogger();
        if (!await EnsureLoginAsync(cancellationToken))
            return false;
        try
        {
            var url = $"{baseUrl}/api/v2/torrents/add";
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            if (!string.IsNullOrWhiteSpace(cookie))
                req.Headers.Add("Cookie", cookie);
            var form = new MultipartFormDataContent { { new StringContent(magnetUrl), "urls" } };
            if (!string.IsNullOrWhiteSpace(savePath))
            {
                form.Add(new StringContent(savePath), "savepath");
            }
            req.Content = form;
            logger.LogDebug("[qBittorrent] POST {Url} (add magnet)", url);
            serviceLogger.Info($"POST {url} (add magnet)");
            var resp = await httpClient.SendAsync(req, cancellationToken);
            var body = await resp.Content.ReadAsStringAsync(cancellationToken);
            var ok = resp.IsSuccessStatusCode;
            if (!ok)
            {
                logger.LogWarning(
                    "[qBittorrent] Add magnet failed HTTP {Status}. Body: {Body}",
                    (int)resp.StatusCode,
                    body
                );
                serviceLogger.Warn($"Add magnet failed HTTP {(int)resp.StatusCode}. Body: {body}");
                return false;
            }
            // qBittorrent returns 200 with body "Ok." on success, and may return other strings (e.g., "Fails.") on failure
            var trimmed = (body ?? string.Empty).Trim();
            var accepted =
                string.IsNullOrEmpty(trimmed)
                || trimmed.Equals("Ok.", StringComparison.OrdinalIgnoreCase)
                || trimmed.Equals("Ok", StringComparison.OrdinalIgnoreCase);
            if (!accepted)
            {
                logger.LogWarning(
                    "[qBittorrent] Add magnet returned unexpected body: '{Body}'",
                    trimmed
                );
                serviceLogger.Warn($"Add magnet returned unexpected body: '{trimmed}'");
            }
            return accepted;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "qBittorrent add magnet failed");
            serviceLogger.Error($"Add magnet exception: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> AddByUrlAsync(
        string torrentUrl,
        string? savePath,
        CancellationToken cancellationToken
    )
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
            return false;
        var serviceLogger = await GetLogger();
        if (!await EnsureLoginAsync(cancellationToken))
            return false;
        try
        {
            var url = $"{baseUrl}/api/v2/torrents/add";
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            if (!string.IsNullOrWhiteSpace(cookie))
                req.Headers.Add("Cookie", cookie);
            var form = new MultipartFormDataContent { { new StringContent(torrentUrl), "urls" } };
            if (!string.IsNullOrWhiteSpace(savePath))
            {
                form.Add(new StringContent(savePath), "savepath");
            }
            req.Content = form;
            logger.LogDebug("[qBittorrent] POST {Url} (add by url)", url);
            serviceLogger.Info($"POST {url} (add by url)");
            var resp = await httpClient.SendAsync(req, cancellationToken);
            var body = await resp.Content.ReadAsStringAsync(cancellationToken);
            var ok = resp.IsSuccessStatusCode;
            if (!ok)
            {
                logger.LogWarning(
                    "[qBittorrent] Add by url failed HTTP {Status}. Body: {Body}",
                    (int)resp.StatusCode,
                    body
                );
                serviceLogger.Warn($"Add by url failed HTTP {(int)resp.StatusCode}. Body: {body}");
                return false;
            }
            var trimmed = (body ?? string.Empty).Trim();
            var accepted =
                string.IsNullOrEmpty(trimmed)
                || trimmed.Equals("Ok.", StringComparison.OrdinalIgnoreCase)
                || trimmed.Equals("Ok", StringComparison.OrdinalIgnoreCase);
            if (!accepted)
            {
                logger.LogWarning(
                    "[qBittorrent] Add by url returned unexpected body: '{Body}'",
                    trimmed
                );
                serviceLogger.Warn($"Add by url returned unexpected body: '{trimmed}'");
            }
            return accepted;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "qBittorrent add by url failed");
            serviceLogger.Error($"Add by url exception: {ex.Message}");
            return false;
        }
    }

    public async Task<(bool ok, string message)> TestConnectivityAsync(
        CancellationToken cancellationToken
    )
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
            return (false, "BaseUrl not configured");
        var serviceLogger = await GetLogger();

        var url = baseUrl + "/api/v2/app/webapiVersion";
        try
        {
            // Ensure we have a session cookie; newer qBittorrent setups may require auth even for this endpoint
            var loggedIn = await EnsureLoginAsync(cancellationToken);
            if (!loggedIn)
            {
                serviceLogger.Warn("Login failed before connectivity test");
            }
            serviceLogger.Info($"Test connectivity: GET {url}");
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrWhiteSpace(cookie))
                req.Headers.Add("Cookie", cookie);
            var resp = await httpClient.SendAsync(req, cancellationToken);
            var msg = $"HTTP {(int)resp.StatusCode}";
            serviceLogger.Info($"Result: {msg}");
            return (resp.IsSuccessStatusCode, msg);
        }
        catch (Exception ex)
        {
            serviceLogger.Error($"Exception: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async Task<IReadOnlyList<TorrentInfo>> GetTorrentsAsync(
        CancellationToken cancellationToken
    )
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
            return Array.Empty<TorrentInfo>();
        var serviceLogger = await GetLogger();
        if (!await EnsureLoginAsync(cancellationToken))
            return Array.Empty<TorrentInfo>();

        try
        {
            var url = $"{baseUrl}/api/v2/torrents/info?filter=all";
            serviceLogger.Info($"GET {url} (list torrents)");
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrWhiteSpace(cookie))
                req.Headers.Add("Cookie", cookie);
            var resp = await httpClient.SendAsync(req, cancellationToken);
            var json = await resp.Content.ReadAsStringAsync(cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "[qBittorrent] List torrents failed HTTP {Status}. Body: {Body}",
                    (int)resp.StatusCode,
                    json
                );
                serviceLogger.Warn(
                    $"List torrents failed HTTP {(int)resp.StatusCode}. Body: {json}"
                );
                return Array.Empty<TorrentInfo>();
            }
            var list = new List<TorrentInfo>();
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var t in doc.RootElement.EnumerateArray())
                    {
                        string hash = t.TryGetProperty("hash", out var ph)
                            ? ph.GetString() ?? string.Empty
                            : string.Empty;
                        string name = t.TryGetProperty("name", out var pn)
                            ? pn.GetString() ?? string.Empty
                            : string.Empty;
                        double progress = t.TryGetProperty("progress", out var pp)
                            ? pp.GetDouble()
                            : 0;
                        string? state = t.TryGetProperty("state", out var ps)
                            ? ps.GetString()
                            : null;
                        string? savePath = t.TryGetProperty("save_path", out var sp)
                            ? sp.GetString()
                            : null;
                        string? contentPath = t.TryGetProperty("content_path", out var cp)
                            ? cp.GetString()
                            : null;
                        list.Add(
                            new TorrentInfo(hash, name, progress, state, savePath, contentPath)
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[qBittorrent] Failed to parse torrents list");
                serviceLogger.Warn($"Failed to parse torrents list: {ex.Message}");
                return Array.Empty<TorrentInfo>();
            }
            return list;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "qBittorrent list torrents failed");
            (await GetLogger()).Error($"List torrents exception: {ex.Message}");
            return Array.Empty<TorrentInfo>();
        }
    }

    public async Task<bool> SetLocationAsync(
        string hash,
        string newLocation,
        CancellationToken cancellationToken
    )
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
            return false;
        var serviceLogger = await GetLogger();
        if (!await EnsureLoginAsync(cancellationToken))
            return false;

        try
        {
            var url = $"{baseUrl}/api/v2/torrents/setLocation";
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            if (!string.IsNullOrWhiteSpace(cookie))
                req.Headers.Add("Cookie", cookie);
            var form = new FormUrlEncodedContent(
                new[]
                {
                    new KeyValuePair<string, string>("hashes", hash),
                    new KeyValuePair<string, string>("location", newLocation),
                }
            );
            req.Content = form;
            logger.LogDebug("[qBittorrent] POST {Url} (setLocation)", url);
            serviceLogger.Info($"POST {url} (setLocation) hashes={hash} location={newLocation}");
            var resp = await httpClient.SendAsync(req, cancellationToken);
            var body = await resp.Content.ReadAsStringAsync(cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "[qBittorrent] setLocation failed HTTP {Status}. Body: {Body}",
                    (int)resp.StatusCode,
                    body
                );
                serviceLogger.Warn($"setLocation failed HTTP {(int)resp.StatusCode}. Body: {body}");
                return false;
            }
            var trimmed = (body ?? string.Empty).Trim();
            var ok =
                trimmed.Equals("Ok.", StringComparison.OrdinalIgnoreCase)
                || trimmed.Equals("Ok", StringComparison.OrdinalIgnoreCase);
            if (!ok)
            {
                logger.LogWarning(
                    "[qBittorrent] setLocation returned unexpected body: '{Body}'",
                    trimmed
                );
                serviceLogger.Warn($"setLocation returned unexpected body: '{trimmed}'");
            }
            return ok;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "qBittorrent setLocation failed");
            serviceLogger.Error($"setLocation exception: {ex.Message}");
            return false;
        }
    }
}
