using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicGQL.Features.ListenBrainz;

namespace MusicGQL.Integration.ListenBrainz;

public class ListenBrainzSimilarityClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ListenBrainzSimilarityClient> _logger;
    private readonly ListenBrainzConfiguration _config;

    public ListenBrainzSimilarityClient(
        HttpClient httpClient,
        ILogger<ListenBrainzSimilarityClient> logger,
        IOptions<ListenBrainzConfiguration> config
    )
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config.Value;
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
        if (!_httpClient.DefaultRequestHeaders.Accept.Any(h => h.MediaType == "application/json"))
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }
        if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MusicGQL/1.0 (+https://github.com/yourusername/mmusic)");
        }
    }

    public async Task<List<SimilarArtistResponse>> GetSimilarArtistsByMbidAsync(string artistMbid, int limit = 12, CancellationToken cancellationToken = default)
    {
        try
        {
            // Try known JSON endpoints, fall back to Labs (HTML) only if JSON parseable
            var algorithm = "session_based_days_9000_session_300_contribution_5_threshold_15_limit_50_skip_30";
            var candidateUrls = new[]
            {
                // Official API variant 1
                $"https://api.listenbrainz.org/1/similar-artists?artist_mbid={Uri.EscapeDataString(artistMbid)}",
                // Official API variant 1 (plural param)
                $"https://api.listenbrainz.org/1/similar-artists?artist_mbids={Uri.EscapeDataString(artistMbid)}",
                // Official API variant 2
                $"https://api.listenbrainz.org/1/similar-artists/artist/{Uri.EscapeDataString(artistMbid)}",
                // Alternate artist path variants we have seen in docs/community posts
                $"https://api.listenbrainz.org/1/artist/{Uri.EscapeDataString(artistMbid)}/similar-artists",
                $"https://api.listenbrainz.org/1/artist/{Uri.EscapeDataString(artistMbid)}/similar",
                // Labs (often HTML, keep last)
                $"https://labs.api.listenbrainz.org/similar-artists?artist_mbid={Uri.EscapeDataString(artistMbid)}&algorithm={Uri.EscapeDataString(algorithm)}&format=json",
                $"https://labs.api.listenbrainz.org/artist/{Uri.EscapeDataString(artistMbid)}/similar-artists?format=json"
            };

            foreach (var url in candidateUrls)
            {
                try
                {
                    _logger.LogInformation("[ListenBrainzSimilarityClient] Fetching similar artists from {Url}", url);
                    using var resp = await _httpClient.GetAsync(url, cancellationToken);
                    var contentType = resp.Content.Headers.ContentType?.MediaType ?? "";
                    var text = await resp.Content.ReadAsStringAsync(cancellationToken);
                    if (!resp.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("[ListenBrainzSimilarityClient] Non-success status {Status} for {Url}", resp.StatusCode, url);
                        continue;
                    }

                    // Heuristic: skip HTML responses (unless format=json forced)
                    if (!url.Contains("format=json", StringComparison.OrdinalIgnoreCase) && text.StartsWith("<"))
                    {
                        _logger.LogWarning("[ListenBrainzSimilarityClient] Response appears to be HTML (content-type: {CT}). Skipping {Url}", contentType, url);
                        continue;
                    }

                    // For Labs responses that look like JSON, log a short preview to help refine the mapper
                    if (url.Contains("labs.api.listenbrainz.org", StringComparison.OrdinalIgnoreCase))
                    {
                        var preview = text.Length > 500 ? text.Substring(0, 500) : text;
                        _logger.LogInformation("[ListenBrainzSimilarityClient] Labs JSON body preview (first 500 chars): {Preview}", preview);
                    }

                    // Attempt direct array shape
                    try
                    {
                        var res = System.Text.Json.JsonSerializer.Deserialize<List<SimilarArtistResponse>>(text, new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (res != null)
                        {
                            return res
                                .Where(r => !string.IsNullOrWhiteSpace(r.artist_mbid) || !string.IsNullOrWhiteSpace(r.artist_name))
                                .OrderByDescending(r => r.score)
                                .Take(limit)
                                .ToList();
                        }
                    }
                    catch
                    {
                        // fall through to document probing
                    }

                    // Attempt to probe JSON document for a property containing the list
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(text);
                        // Try common containers: "payload", "results", "data"
                        foreach (var key in new[] { "payload", "results", "data", "similar_artists", "similar-artists" })
                        {
                            if (doc.RootElement.TryGetProperty(key, out var prop) && prop.ValueKind == System.Text.Json.JsonValueKind.Array)
                            {
                                var list = new List<SimilarArtistResponse>();
                                foreach (var el in prop.EnumerateArray())
                                {
                                    string mb = el.TryGetProperty("artist_mbid", out var v1) ? v1.GetString() ?? string.Empty :
                                                el.TryGetProperty("mbid", out var v2) ? v2.GetString() ?? string.Empty : string.Empty;
                                    string name = el.TryGetProperty("artist_name", out var n1) ? n1.GetString() ?? string.Empty :
                                                  el.TryGetProperty("name", out var n2) ? n2.GetString() ?? string.Empty : string.Empty;
                                    double score = el.TryGetProperty("score", out var s1) && s1.TryGetDouble(out var d) ? d : 0.0;
                                    if (!string.IsNullOrWhiteSpace(mb) || !string.IsNullOrWhiteSpace(name))
                                    {
                                        list.Add(new SimilarArtistResponse { artist_mbid = mb, artist_name = name, score = score });
                                    }
                                }
                                if (list.Count > 0)
                                {
                                    return list.OrderByDescending(r => r.score).Take(limit).ToList();
                                }
                            }
                        }
                    }
                    catch
                    {
                        // ignore and try next URL
                    }
                }
                catch (Exception exUrl)
                {
                    _logger.LogWarning(exUrl, "[ListenBrainzSimilarityClient] Error trying endpoint {Url}", url);
                    continue;
                }
            }

            return [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[ListenBrainzSimilarityClient] Failed to fetch similar artists for {Mbid}", artistMbid);
            return [];
        }
    }
}

public class SimilarArtistResponse
{
    public string artist_mbid { get; set; } = string.Empty;
    public string artist_name { get; set; } = string.Empty;
    public double score { get; set; }
}


