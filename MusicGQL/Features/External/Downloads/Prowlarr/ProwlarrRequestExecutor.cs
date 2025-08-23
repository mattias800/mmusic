using System.Net;
using Microsoft.Extensions.Options;
using MusicGQL.Features.Downloads.Services;

namespace MusicGQL.Features.External.Downloads.Prowlarr;

internal sealed class ProwlarrRequestExecutor(HttpClient httpClient, IOptions<Configuration.ProwlarrOptions> options, ILogger logger)
{
    internal record Result(bool Success, string? Json, HttpStatusCode StatusCode, string? ReasonPhrase, TimeSpan Duration, string HeadersString);

    public async Task<Result> GetJsonAsync(string url, CancellationToken cancellationToken, IDownloadLogger? relLogger = null)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Accept.Clear();
        req.Headers.Accept.ParseAdd("application/json");
        var apiKey = options.Value.ApiKey;
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            try { req.Headers.Add("X-Api-Key", apiKey); } catch { }
        }

        var headersStr = string.Join(", ", req.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"));
        logger.LogInformation("[Prowlarr] GET {Url}", url);
        try { relLogger?.Info($"[Prowlarr] GET {url}"); } catch { }
        if (options.Value.EnableDetailedLogging)
        {
            logger.LogInformation("[Prowlarr] Headers: {Headers}", headersStr);
            try { relLogger?.Info($"[Prowlarr] Headers: {headersStr}"); } catch { }
        }

        var startTime = DateTime.UtcNow;
        var resp = await httpClient.SendAsync(req, cancellationToken);
        var duration = DateTime.UtcNow - startTime;
        logger.LogInformation("[Prowlarr] RESPONSE: Status {Status} in {Duration:0.00}s", (int)resp.StatusCode, duration.TotalSeconds);
        try { relLogger?.Info($"[Prowlarr] RESPONSE: Status {(int)resp.StatusCode} in {duration.TotalSeconds:0.00}s"); } catch { }

        if (!resp.IsSuccessStatusCode)
        {
            string? body = null;
            try { body = await resp.Content.ReadAsStringAsync(cancellationToken); } catch { }
            if (!string.IsNullOrWhiteSpace(body) && body.Length > 500) body = body[..500] + "…";
            var reasonPhrase = resp.ReasonPhrase ?? "";
            logger.LogInformation("[Prowlarr] ❌ ERROR RESPONSE: HTTP {Status} {Reason} - {Body}", (int)resp.StatusCode, reasonPhrase, body ?? "null");
            try { relLogger?.Warn($"[Prowlarr] ❌ ERROR RESPONSE: HTTP {(int)resp.StatusCode} {reasonPhrase} - {body ?? "null"}"); } catch { }
            return new Result(false, null, resp.StatusCode, resp.ReasonPhrase, duration, headersStr);
        }

        var json = await resp.Content.ReadAsStringAsync(cancellationToken);
        return new Result(true, json, resp.StatusCode, resp.ReasonPhrase, duration, headersStr);
    }
}

