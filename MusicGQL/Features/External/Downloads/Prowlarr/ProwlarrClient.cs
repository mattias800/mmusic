using System.Net.Http.Json;
using System.Text.Json;
using MusicGQL.Features.External.Downloads.Prowlarr.Configuration;
using Microsoft.Extensions.Options;

namespace MusicGQL.Features.External.Downloads.Prowlarr;

public class ProwlarrClient(HttpClient httpClient, IOptions<ProwlarrOptions> options, ILogger<ProwlarrClient> logger)
{
    public async Task<IReadOnlyList<ProwlarrRelease>> SearchAlbumAsync(string artistName, string releaseTitle, CancellationToken cancellationToken)
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
        {
            return Array.Empty<ProwlarrRelease>();
        }

        // Build search query with quality preferences
        var qualityTerms = new[] { "320", "FLAC", "lossless", "CD", "vinyl", "24bit", "16bit" };
        var baseQuery = artistName + " " + releaseTitle;
        
        // Create multiple search variants with different quality priorities
        var searchQueries = new List<string>
        {
            // High quality first
            baseQuery + " " + string.Join(" ", qualityTerms),
            baseQuery + " FLAC 320",
            baseQuery + " lossless",
            baseQuery + " 320",
            // Fallback to base query
            baseQuery
        };
        
        logger.LogInformation("[Prowlarr] Will try search queries in order: {Queries}", string.Join(" | ", searchQueries));
        
        // Try each search query until we find results
        foreach (var searchQuery in searchQueries)
        {
            var results = await TrySearchWithQueryAsync(searchQuery, cancellationToken);
            if (results.Count > 0)
            {
                logger.LogInformation("[Prowlarr] Found {Count} results with query: '{Query}'", results.Count, searchQuery);
                return results;
            }
        }
        
        logger.LogInformation("[Prowlarr] No results found with any search query");
        return Array.Empty<ProwlarrRelease>();
    }
    
    private async Task<IReadOnlyList<ProwlarrRelease>> TrySearchWithQueryAsync(string query, CancellationToken cancellationToken)
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
        {
            return Array.Empty<ProwlarrRelease>();
        }

        // Try a few parameter variants to handle API changes/configs
        var candidateUrls = new List<string>
        {
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey)}&query={Uri.EscapeDataString(query)}",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey)}&query={Uri.EscapeDataString(query)}&type=search",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey)}&query={Uri.EscapeDataString(query)}&categories=3000&categories=3010&categories=3030",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey)}&query={Uri.EscapeDataString(query)}&categories=3000&categories=3010&categories=3030&limit=50",
            // Alternate param names some setups accept
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey)}&term={Uri.EscapeDataString(query)}",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey)}&term={Uri.EscapeDataString(query)}&categories=3000&categories=3010&categories=3030",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey)}&term={Uri.EscapeDataString(query)}&categories=3000&categories=3010&categories=3030&limit=50",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey)}&q={Uri.EscapeDataString(query)}",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey)}&Query={Uri.EscapeDataString(query)}",
        };

        foreach (var url in candidateUrls)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.Accept.Clear();
                req.Headers.Accept.ParseAdd("application/json");
                // Some setups prefer API key via header
                try { req.Headers.Add("X-Api-Key", apiKey); } catch { }
                logger.LogInformation("[Prowlarr] GET {Url}", url);
                var resp = await httpClient.SendAsync(req, cancellationToken);
                if (!resp.IsSuccessStatusCode)
                {
                    string? body = null;
                    try { body = await resp.Content.ReadAsStringAsync(cancellationToken); } catch { }
                    if (!string.IsNullOrWhiteSpace(body) && body.Length > 500) body = body[..500] + "…";
                    logger.LogInformation("[Prowlarr] GET HTTP {Status} for {Url}. Body: {Body}", (int)resp.StatusCode, url, body);
                    continue;
                }
                var json = await resp.Content.ReadAsStringAsync(cancellationToken);
                var preview = json.Length > 600 ? json[..600] + "…" : json;
                logger.LogInformation("[Prowlarr] GET body preview: {Preview}", preview);
                using var doc = JsonDocument.Parse(json);
                var list = ParseProwlarrResults(doc.RootElement);
                logger.LogInformation("[Prowlarr] Parsed {Count} results from GET", list.Count);
                if (list.Count == 0)
                {
                    logger.LogInformation("[Prowlarr] No results from this GET variant; trying next, if any.");
                    try { LogRootShape(doc.RootElement, logger); } catch { }
                    continue;
                }
                return list;
            }
            catch (HttpRequestException ex)
            {
                logger.LogDebug(ex, "Prowlarr search HTTP exception for {Url}", url);
                continue;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Prowlarr search failed for {Url}", url);
                continue;
            }
        }
        // Try POST variant some setups require
        try
        {
            var baseUrl2 = options.Value.BaseUrl!.TrimEnd('/');
            var apiKey2 = options.Value.ApiKey!;
            var url = $"{baseUrl2}/api/v1/search?apikey={Uri.EscapeDataString(apiKey2)}";
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Accept.Clear();
            req.Headers.Accept.ParseAdd("application/json");
            try { req.Headers.Add("X-Api-Key", apiKey2); } catch { }
            var payload = new
            {
                query = query,
                type = "search",
                categories = new[] { 3000, 3010, 3030 },
                indexerIds = Array.Empty<int>()
            };
            req.Content = new StringContent(JsonSerializer.Serialize(payload));
            req.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            logger.LogInformation("[Prowlarr] POST {Url} with payload query='{Query}'", url, query);
            var resp = await httpClient.SendAsync(req, cancellationToken);
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync(cancellationToken);
                var preview = json.Length > 600 ? json[..600] + "…" : json;
                logger.LogInformation("[Prowlarr] POST body preview: {Preview}", preview);
                using var doc = JsonDocument.Parse(json);
                var list = ParseProwlarrResults(doc.RootElement);
                logger.LogInformation("[Prowlarr] Parsed {Count} results from POST", list.Count);
                if (list.Count == 0)
                {
                    logger.LogInformation("[Prowlarr] No results from POST variant.");
                    try { LogRootShape(doc.RootElement, logger); } catch { }
                }
                else
                {
                    return list;
                }
            }
            else
            {
                string? body = null;
                try { body = await resp.Content.ReadAsStringAsync(cancellationToken); } catch { }
                if (!string.IsNullOrWhiteSpace(body) && body.Length > 500) body = body[..500] + "…";
                logger.LogInformation("[Prowlarr] POST search HTTP {Status}. Body: {Body}", (int)resp.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Prowlarr POST search failed");
        }
        logger.LogWarning("Prowlarr search failed for all URL variants for query '{Query}'", query);
        return Array.Empty<ProwlarrRelease>();
    }

    private static List<ProwlarrRelease> ParseProwlarrResults(JsonElement root)
    {
        // Root can be array or object with Results/results
        JsonElement array = root;
        if (root.ValueKind == JsonValueKind.Object)
        {
            if (!TryGetPropertyCI(root, "results", out array) && !TryGetPropertyCI(root, "Results", out array))
            {
                // Try direct array property 'Data'
                if (!TryGetPropertyCI(root, "data", out array) && !TryGetPropertyCI(root, "Data", out array))
                {
                    array = root;
                }
            }
        }
        if (array.ValueKind != JsonValueKind.Array)
        {
            return new List<ProwlarrRelease>();
        }
        var list = new List<ProwlarrRelease>();
        foreach (var item in array.EnumerateArray())
        {
            string? title = GetStringCI(item, "title") ?? GetStringCI(item, "Title");
            string? guid = GetStringCI(item, "guid") ?? GetStringCI(item, "Guid");
            string? magnet = GetStringCI(item, "magnetUrl") ?? GetStringCI(item, "MagnetUrl");
            string? downloadUrl = GetStringCI(item, "downloadUrl") ?? GetStringCI(item, "DownloadUrl")
                ?? GetStringCI(item, "link") ?? GetStringCI(item, "Link");
            long? size = GetInt64CI(item, "size") ?? GetInt64CI(item, "Size");
            int? indexerId = null;
            if (TryGetPropertyCI(item, "indexerId", out var idx) && idx.ValueKind == JsonValueKind.Number)
            {
                if (idx.TryGetInt32(out var i32)) indexerId = i32;
            }
            list.Add(new ProwlarrRelease(title, guid, magnet, downloadUrl, size, indexerId));
        }
        return list;
    }

    private static void LogRootShape(JsonElement root, ILogger logger)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            logger.LogDebug("[Prowlarr] Root is array; first item keys: {Keys}",
                root.GetArrayLength() > 0 ? string.Join(",", root[0].EnumerateObject().Select(p => p.Name)) : "(empty)");
            return;
        }
        if (root.ValueKind == JsonValueKind.Object)
        {
            var keys = root.EnumerateObject().Select(p => p.Name).ToList();
            logger.LogDebug("[Prowlarr] Root is object; keys: {Keys}", string.Join(",", keys));
            foreach (var k in keys)
            {
                if (TryGetPropertyCI(root, k, out var v) && v.ValueKind == JsonValueKind.Array && v.GetArrayLength() > 0)
                {
                    var innerKeys = v[0].EnumerateObject().Select(p => p.Name);
                    logger.LogDebug("[Prowlarr] First element keys of '{Key}': {InnerKeys}", k, string.Join(",", innerKeys));
                }
            }
        }
        else
        {
            logger.LogDebug("[Prowlarr] Root is {Kind}", root.ValueKind);
        }
    }

    private static bool TryGetPropertyCI(JsonElement obj, string name, out JsonElement value)
    {
        if (obj.ValueKind != JsonValueKind.Object)
        {
            value = default;
            return false;
        }
        foreach (var prop in obj.EnumerateObject())
        {
            if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = prop.Value;
                return true;
            }
        }
        value = default;
        return false;
    }

    private static string? GetStringCI(JsonElement obj, string name)
    {
        if (TryGetPropertyCI(obj, name, out var v) && v.ValueKind == JsonValueKind.String)
        {
            return v.GetString();
        }
        return null;
    }

    private static long? GetInt64CI(JsonElement obj, string name)
    {
        if (TryGetPropertyCI(obj, name, out var v) && v.ValueKind == JsonValueKind.Number)
        {
            if (v.TryGetInt64(out var i64)) return i64;
        }
        return null;
    }
}

public record ProwlarrRelease(string? Title, string? Guid, string? MagnetUrl, string? DownloadUrl, long? Size, int? IndexerId);


