using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicGQL.Features.ListenBrainz;

namespace MusicGQL.Integration.ListenBrainz;

public class ListenBrainzPopularityClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ListenBrainzPopularityClient> _logger;
    private readonly ListenBrainzConfiguration _config;

    public ListenBrainzPopularityClient(
        HttpClient httpClient,
        ILogger<ListenBrainzPopularityClient> logger,
        IOptions<ListenBrainzConfiguration> config
    )
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config.Value;

        // Set default headers
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "MusicGQL/1.0");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.PopularityTimeoutSeconds);

        _logger.LogInformation(
            "[ListenBrainzPopularityClient] Initialized with timeout: {Timeout}s, max retries: {MaxRetries}, retry delay: {RetryDelay}s",
            _config.PopularityTimeoutSeconds,
            _config.PopularityMaxRetries,
            _config.PopularityRetryDelaySeconds
        );
    }

    public async Task<List<ListenBrainzTopRecording>> GetTopRecordingsForArtistAsync(
        string artistMbid,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "[ListenBrainzPopularityClient] Starting to fetch top recordings for artist MBID: {ArtistMbid}",
            artistMbid
        );

        for (int attempt = 1; attempt <= _config.PopularityMaxRetries; attempt++)
        {
            try
            {
                var url = $"{_config.BaseUrl}/1/popularity/top-recordings-for-artist/{artistMbid}";
                _logger.LogInformation(
                    "[ListenBrainzPopularityClient] Attempt {Attempt}/{MaxRetries}: Fetching from URL: {Url}",
                    attempt,
                    _config.PopularityMaxRetries,
                    url
                );

                _logger.LogDebug(
                    "[ListenBrainzPopularityClient] HTTP Headers: {Headers}",
                    string.Join(
                        ", ",
                        _httpClient.DefaultRequestHeaders.Select(h =>
                            $"{h.Key}: {string.Join(", ", h.Value)}"
                        )
                    )
                );

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _httpClient.GetAsync(url, cancellationToken);
                stopwatch.Stop();

                _logger.LogInformation(
                    "[ListenBrainzPopularityClient] HTTP response received in {ElapsedMs}ms. Status: {StatusCode}, Content-Length: {ContentLength}",
                    stopwatch.ElapsedMilliseconds,
                    response.StatusCode,
                    response.Content.Headers.ContentLength
                );

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning(
                        "[ListenBrainzPopularityClient] HTTP error response for artist {ArtistMbid}. Status: {StatusCode}, Content: {ErrorContent} (attempt {Attempt})",
                        artistMbid,
                        response.StatusCode,
                        errorContent,
                        attempt
                    );

                    _logger.LogInformation(
                        "[ListenBrainzPopularityClient] Full error response for artist {ArtistMbid}: Status {StatusCode}, Content: {ErrorContent}",
                        artistMbid,
                        response.StatusCode,
                        errorContent
                    );

                    if (attempt < _config.PopularityMaxRetries)
                    {
                        _logger.LogInformation(
                            "[ListenBrainzPopularityClient] Retrying in {RetryDelay}s...",
                            _config.PopularityRetryDelaySeconds
                        );
                        await Task.Delay(
                            TimeSpan.FromSeconds(_config.PopularityRetryDelaySeconds),
                            cancellationToken
                        );
                        continue;
                    }

                    _logger.LogError(
                        "[ListenBrainzPopularityClient] All retry attempts failed for artist {ArtistMbid}. Final status: {StatusCode}",
                        artistMbid,
                        response.StatusCode
                    );
                    return [];
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning(
                        "[ListenBrainzPopularityClient] Empty response content for artist {ArtistMbid} (attempt {Attempt})",
                        artistMbid,
                        attempt
                    );

                    if (attempt < _config.PopularityMaxRetries)
                    {
                        _logger.LogInformation(
                            "[ListenBrainzPopularityClient] Retrying in {RetryDelay}s...",
                            _config.PopularityRetryDelaySeconds
                        );
                        await Task.Delay(
                            TimeSpan.FromSeconds(_config.PopularityRetryDelaySeconds),
                            cancellationToken
                        );
                        continue;
                    }

                    return [];
                }

                _logger.LogInformation(
                    "[ListenBrainzPopularityClient] Attempting to deserialize response for artist {ArtistMbid}. Content length: {ContentLength}",
                    artistMbid,
                    content.Length
                );

                var recordings = JsonSerializer.Deserialize<List<ListenBrainzTopRecording>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (recordings == null)
                {
                    _logger.LogWarning(
                        "[ListenBrainzPopularityClient] Failed to deserialize response for artist {ArtistMbid}. Content: {Content} (attempt {Attempt})",
                        artistMbid,
                        content,
                        attempt
                    );

                    if (attempt < _config.PopularityMaxRetries)
                    {
                        _logger.LogInformation(
                            "[ListenBrainzPopularityClient] Retrying in {RetryDelay}s...",
                            _config.PopularityRetryDelaySeconds
                        );
                        await Task.Delay(
                            TimeSpan.FromSeconds(_config.PopularityRetryDelaySeconds),
                            cancellationToken
                        );
                        continue;
                    }

                    return [];
                }

                _logger.LogInformation(
                    "[ListenBrainzPopularityClient] Successfully deserialized {Count} recordings for artist {ArtistMbid} (attempt {Attempt})",
                    recordings.Count,
                    artistMbid,
                    attempt
                );

                // Filter to top 30 tracks by listen count and only log those
                var topRecordings = recordings
                    .OrderByDescending(r => r.TotalListenCount)
                    .Take(30)
                    .ToList();

                _logger.LogInformation(
                    "[ListenBrainzPopularityClient] Filtered to top {TopCount} recordings by listen count (from {TotalCount} total)",
                    topRecordings.Count,
                    recordings.Count
                );

                if (topRecordings.Count > 0)
                {
                    var firstRecording = topRecordings[0];
                    _logger.LogInformation(
                        "[ListenBrainzPopularityClient] Top recording: '{RecordingName}' by '{ArtistName}' (MBID: {RecordingMbid})",
                        firstRecording.RecordingName,
                        firstRecording.ArtistName,
                        firstRecording.RecordingMbid
                    );

                    // Add more detailed logging to debug the deserialization
                    _logger.LogInformation(
                        "[ListenBrainzPopularityClient] Top recording details - Title: '{Title}', Artist: '{Artist}', MBID: '{Mbid}', Length: {Length}, ListenCount: {ListenCount}, UserCount: {UserCount}",
                        firstRecording.RecordingName,
                        firstRecording.ArtistName,
                        firstRecording.RecordingMbid,
                        firstRecording.Length,
                        firstRecording.TotalListenCount,
                        firstRecording.TotalUserCount
                    );
                }

                return topRecordings;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogWarning(
                    "[ListenBrainzPopularityClient] Request timeout for artist {ArtistMbid} (attempt {Attempt}/{MaxRetries}). Timeout: {Timeout}s",
                    artistMbid,
                    attempt,
                    _config.PopularityMaxRetries,
                    _config.PopularityTimeoutSeconds
                );

                if (attempt < _config.PopularityMaxRetries)
                {
                    _logger.LogInformation(
                        "[ListenBrainzPopularityClient] Retrying in {RetryDelay}s...",
                        _config.PopularityRetryDelaySeconds
                    );
                    await Task.Delay(
                        TimeSpan.FromSeconds(_config.PopularityRetryDelaySeconds),
                        cancellationToken
                    );
                    continue;
                }

                _logger.LogError(
                    "[ListenBrainzPopularityClient] All retry attempts timed out for artist {ArtistMbid}",
                    artistMbid
                );
                return [];
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(
                    "[ListenBrainzPopularityClient] HTTP request exception for artist {ArtistMbid} (attempt {Attempt}/{MaxRetries}): {Message}",
                    artistMbid,
                    attempt,
                    _config.PopularityMaxRetries,
                    ex.Message
                );

                if (attempt < _config.PopularityMaxRetries)
                {
                    _logger.LogInformation(
                        "[ListenBrainzPopularityClient] Retrying in {RetryDelay}s...",
                        _config.PopularityRetryDelaySeconds
                    );
                    await Task.Delay(
                        TimeSpan.FromSeconds(_config.PopularityRetryDelaySeconds),
                        cancellationToken
                    );
                    continue;
                }

                _logger.LogError(
                    "[ListenBrainzPopularityClient] All retry attempts failed with HTTP request exception for artist {ArtistMbid}: {Message}",
                    artistMbid,
                    ex.Message
                );
                return [];
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(
                    "[ListenBrainzPopularityClient] JSON deserialization exception for artist {ArtistMbid} (attempt {Attempt}/{MaxRetries}): {Message}",
                    artistMbid,
                    attempt,
                    _config.PopularityMaxRetries,
                    ex.Message
                );

                if (attempt < _config.PopularityMaxRetries)
                {
                    _logger.LogInformation(
                        "[ListenBrainzPopularityClient] Retrying in {RetryDelay}s...",
                        _config.PopularityRetryDelaySeconds
                    );
                    await Task.Delay(
                        TimeSpan.FromSeconds(_config.PopularityRetryDelaySeconds),
                        cancellationToken
                    );
                    continue;
                }

                _logger.LogError(
                    "[ListenBrainzPopularityClient] All retry attempts failed with JSON exception for artist {ArtistMbid}: {Message}",
                    artistMbid,
                    ex.Message
                );
                return [];
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[ListenBrainzPopularityClient] Unexpected exception for artist {ArtistMbid} (attempt {Attempt}/{MaxRetries})",
                    artistMbid,
                    attempt,
                    _config.PopularityMaxRetries
                );

                if (attempt < _config.PopularityMaxRetries)
                {
                    _logger.LogInformation(
                        "[ListenBrainzPopularityClient] Retrying in {RetryDelay}s...",
                        _config.PopularityRetryDelaySeconds
                    );
                    await Task.Delay(
                        TimeSpan.FromSeconds(_config.PopularityRetryDelaySeconds),
                        cancellationToken
                    );
                    continue;
                }

                _logger.LogError(
                    "[ListenBrainzPopularityClient] All retry attempts failed with unexpected exception for artist {ArtistMbid}",
                    artistMbid
                );
                return [];
            }
        }

        _logger.LogError(
            "[ListenBrainzPopularityClient] Exhausted all retry attempts for artist {ArtistMbid}",
            artistMbid
        );
        return [];
    }
}

public class ListenBrainzTopRecording
{
    [JsonPropertyName("artist_mbids")]
    public List<string> ArtistMbids { get; set; } = [];

    [JsonPropertyName("artist_name")]
    public string ArtistName { get; set; } = string.Empty;

    [JsonPropertyName("caa_id")]
    public long? CaaId { get; set; }

    [JsonPropertyName("caa_release_mbid")]
    public string? CaaReleaseMbid { get; set; }

    [JsonPropertyName("length")]
    public int? Length { get; set; }

    [JsonPropertyName("recording_mbid")]
    public string RecordingMbid { get; set; } = string.Empty;

    [JsonPropertyName("recording_name")]
    public string RecordingName { get; set; } = string.Empty;

    [JsonPropertyName("release_color")]
    public ReleaseColor? ReleaseColor { get; set; }

    [JsonPropertyName("release_mbid")]
    public string? ReleaseMbid { get; set; }

    [JsonPropertyName("release_name")]
    public string? ReleaseName { get; set; }

    [JsonPropertyName("total_listen_count")]
    public long TotalListenCount { get; set; }

    [JsonPropertyName("total_user_count")]
    public long TotalUserCount { get; set; }
}

public class ReleaseColor
{
    public int Blue { get; set; }
    public int Green { get; set; }
    public int Red { get; set; }
}
