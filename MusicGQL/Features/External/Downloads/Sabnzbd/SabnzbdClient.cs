using Microsoft.Extensions.Options;
using MusicGQL.Features.External.Downloads.Sabnzbd.Configuration;

namespace MusicGQL.Features.External.Downloads.Sabnzbd;

public class SabnzbdClient(HttpClient httpClient, IOptions<SabnzbdOptions> options, ILogger<SabnzbdClient> logger)
{
    public async Task<bool> AddNzbByUrlAsync(string nzbUrl, string? nzbName, CancellationToken cancellationToken, string? pathOverride = null)
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("[SABnzbd] Missing BaseUrl or ApiKey; cannot send NZB");
            return false;
        }
        var category = options.Value.Category ?? "music";
        try
        {
            var url = $"{baseUrl}/api?mode=addurl&name={Uri.EscapeDataString(nzbUrl)}&apikey={Uri.EscapeDataString(apiKey)}&output=json&cat={Uri.EscapeDataString(category)}";
            if (!string.IsNullOrWhiteSpace(nzbName))
            {
                url += "&nzbname=" + Uri.EscapeDataString(nzbName);
            }
            if (!string.IsNullOrWhiteSpace(pathOverride))
            {
                // Try both keys; SAB will ignore unknown
                url += "&path=" + Uri.EscapeDataString(pathOverride);
                url += "&dir=" + Uri.EscapeDataString(pathOverride);
            }
            logger.LogDebug("[SABnzbd] GET {Url}", url);
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            var resp = await httpClient.SendAsync(req, cancellationToken);
            var body = await resp.Content.ReadAsStringAsync(cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                var trimmed = body?.Length > 500 ? body[..500] + "…" : body;
                logger.LogWarning("[SABnzbd] Add URL failed HTTP {Status}. Body: {Body}", (int)resp.StatusCode, trimmed);
                return false;
            }
            try
            {
                var json = System.Text.Json.JsonDocument.Parse(body);
                var statusProp = json.RootElement.TryGetProperty("status", out var s) ? s : default;
                var okJson = statusProp.ValueKind == System.Text.Json.JsonValueKind.True || (statusProp.ValueKind == System.Text.Json.JsonValueKind.String && string.Equals(statusProp.GetString(), "true", StringComparison.OrdinalIgnoreCase));
                if (!okJson)
                {
                    logger.LogWarning("[SABnzbd] Add URL returned status=false. Body: {Body}", body);
                }
                return okJson;
            }
            catch
            {
                // If not JSON, assume success when HTTP 200
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "SABnzbd addurl failed");
            return false;
        }
    }

    public async Task<bool> AddNzbByContentAsync(byte[] nzbBytes, string fileName, CancellationToken cancellationToken, string? pathOverride = null, string? nzbName = null)
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("[SABnzbd] Missing BaseUrl or ApiKey; cannot send NZB content");
            return false;
        }
        var category = options.Value.Category ?? "music";
        try
        {
            var url = $"{baseUrl}/api?mode=addfile&apikey={Uri.EscapeDataString(apiKey)}&output=json&cat={Uri.EscapeDataString(category)}";
            if (!string.IsNullOrWhiteSpace(nzbName))
            {
                url += "&nzbname=" + Uri.EscapeDataString(nzbName);
            }
            if (!string.IsNullOrWhiteSpace(pathOverride))
            {
                url += "&path=" + Uri.EscapeDataString(pathOverride);
                url += "&dir=" + Uri.EscapeDataString(pathOverride);
            }
            using var form = new MultipartFormDataContent();
            var content = new ByteArrayContent(nzbBytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-nzb");
            form.Add(content, "nzbfile", fileName);
            logger.LogDebug("[SABnzbd] POST {Url} (addfile) filename={File}", url, fileName);
            var resp = await httpClient.PostAsync(url, form, cancellationToken);
            var ok = resp.IsSuccessStatusCode;
            if (!ok)
            {
                string? body = null;
                try { body = await resp.Content.ReadAsStringAsync(cancellationToken); } catch { }
                if (!string.IsNullOrWhiteSpace(body) && body.Length > 500) body = body[..500] + "…";
                logger.LogWarning("[SABnzbd] Add file failed HTTP {Status}. Body: {Body}", (int)resp.StatusCode, body);
            }
            return ok;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[SABnzbd] addfile failed");
            return false;
        }
    }
}


