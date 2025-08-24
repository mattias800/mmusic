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
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
            );
        }
        if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "MusicGQL/1.0 (+https://github.com/yourusername/mmusic)"
            );
        }
    }

    public async Task<List<SimilarArtistResponse>> GetSimilarArtistsByMbidAsync(
        string artistMbid,
        int limit = 12,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // The official API doesn't expose a public similar-artists endpoint.
            // Use the Labs dataset hoster JSON endpoint which accepts POST with required fields.
            var endpoint = _config.SimilarArtistsEndpoint.TrimEnd('/'); // e.g., https://labs.api.listenbrainz.org/similar-artists/json
            var algorithm = string.IsNullOrWhiteSpace(_config.SimilarArtistsAlgorithm)
                ? "session_based_days_9000_session_300_contribution_5_threshold_15_limit_50_skip_30"
                : _config.SimilarArtistsAlgorithm;

            var payload = new[] { new { artist_mbids = new[] { artistMbid }, algorithm } };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            using var content = new StringContent(
                json,
                System.Text.Encoding.UTF8,
                "application/json"
            );
            var req = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };

            _logger.LogInformation(
                "[ListenBrainzSimilarityClient] POST {Endpoint} (algorithm={Algorithm}, artist={Artist}) payload={Payload}",
                endpoint,
                algorithm,
                artistMbid,
                json
            );

            using var resp = await _httpClient.SendAsync(req, cancellationToken);
            var text = await resp.Content.ReadAsStringAsync(cancellationToken);

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "[ListenBrainzSimilarityClient] Non-success status {Status} for {Url}. Body: {Body}",
                    resp.StatusCode,
                    endpoint,
                    text
                );

                // Fallback: try GET /json with query params if POST fails (e.g., rare 400s)
                var getUrl =
                    $"{endpoint}?artist_mbids={Uri.EscapeDataString(artistMbid)}&algorithm={Uri.EscapeDataString(algorithm)}";
                _logger.LogInformation(
                    "[ListenBrainzSimilarityClient] Falling back to GET {Url}",
                    getUrl
                );
                using var getResp = await _httpClient.GetAsync(getUrl, cancellationToken);
                var getText = await getResp.Content.ReadAsStringAsync(cancellationToken);
                if (!getResp.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "[ListenBrainzSimilarityClient] GET fallback also failed: {Status}. Body: {Body}",
                        getResp.StatusCode,
                        getText
                    );
                    return [];
                }
                text = getText;
            }

            // The labs endpoint returns an array of objects: { artist_mbid, name, score, ... }
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            var raw =
                System.Text.Json.JsonSerializer.Deserialize<List<LabsSimilarArtist>>(text, options)
                ?? [];

            var mapped = raw.Where(r =>
                    !string.IsNullOrWhiteSpace(r.artist_mbid) || !string.IsNullOrWhiteSpace(r.name)
                )
                .Select(r => new SimilarArtistResponse
                {
                    artist_mbid = r.artist_mbid ?? string.Empty,
                    artist_name = r.name ?? string.Empty,
                    score = r.score,
                })
                .OrderByDescending(r => r.score)
                .Take(limit)
                .ToList();

            _logger.LogInformation(
                "[ListenBrainzSimilarityClient] Got {Count} similar artists (taking top {Limit})",
                raw.Count,
                mapped.Count
            );
            return mapped;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "[ListenBrainzSimilarityClient] Failed to fetch similar artists for {Mbid}",
                artistMbid
            );
            return [];
        }
    }
}

internal class LabsSimilarArtist
{
    public string? artist_mbid { get; set; }
    public string? name { get; set; }
    public double score { get; set; }
}

public class SimilarArtistResponse
{
    public string artist_mbid { get; set; } = string.Empty;
    public string artist_name { get; set; } = string.Empty;
    public double score { get; set; }
}
