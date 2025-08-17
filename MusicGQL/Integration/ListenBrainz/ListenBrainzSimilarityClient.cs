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
    }

    public async Task<List<SimilarArtistResponse>> GetSimilarArtistsByMbidAsync(string artistMbid, int limit = 12, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use labs endpoint for similar artists
            var algorithm = "session_based_days_9000_session_300_contribution_5_threshold_15_limit_50_skip_30";
            var url = $"https://labs.api.listenbrainz.org/similar-artists?artist_mbid={Uri.EscapeDataString(artistMbid)}&algorithm={Uri.EscapeDataString(algorithm)}";
            _logger.LogInformation("[ListenBrainzSimilarityClient] Fetching similar artists from {Url}", url);

            var res = await _httpClient.GetFromJsonAsync<List<SimilarArtistResponse>>(url, cancellationToken);
            if (res == null)
                return [];
            return res
                .Where(r => !string.IsNullOrWhiteSpace(r.artist_mbid) || !string.IsNullOrWhiteSpace(r.artist_name))
                .OrderByDescending(r => r.score)
                .Take(limit)
                .ToList();
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


