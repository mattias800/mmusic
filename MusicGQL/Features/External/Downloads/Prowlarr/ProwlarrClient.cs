using System.Net.Http.Json;
using System.Text.Json;
using MusicGQL.Features.External.Downloads.Prowlarr.Configuration;
using Microsoft.Extensions.Options;

namespace MusicGQL.Features.External.Downloads.Prowlarr;

public class ProwlarrClient(HttpClient httpClient, IOptions<ProwlarrOptions> options, ILogger<ProwlarrClient> logger)
{
    private async Task<bool> TestConnectivityAsync(CancellationToken cancellationToken)
    {
        try
        {
            var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
                return false;

            // Try a simple ping to the server
            var pingUrl = $"{baseUrl}/api/v1/system/status";
            using var req = new HttpRequestMessage(HttpMethod.Get, pingUrl);
            req.Headers.Accept.Clear();
            req.Headers.Accept.ParseAdd("application/json");
            
            logger.LogDebug("[Prowlarr] Testing connectivity to {Url}", pingUrl);
            var resp = await httpClient.SendAsync(req, cancellationToken);
            var isConnected = resp.IsSuccessStatusCode;
            logger.LogInformation("[Prowlarr] Connectivity test to {BaseUrl}: {Status} ({Connected})", 
                baseUrl, (int)resp.StatusCode, isConnected ? "Connected" : "Failed");
            return isConnected;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[Prowlarr] Connectivity test failed to {BaseUrl}", options.Value.BaseUrl);
            return false;
        }
    }

    public async Task<IReadOnlyList<ProwlarrRelease>> SearchAlbumAsync(string artistName, string releaseTitle, CancellationToken cancellationToken)
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("[Prowlarr] Missing configuration: BaseUrl={BaseUrl}, ApiKey={HasApiKey}", 
                baseUrl ?? "null", !string.IsNullOrWhiteSpace(apiKey));
            return Array.Empty<ProwlarrRelease>();
        }

        logger.LogInformation("[Prowlarr] Starting search for '{Artist} - {Release}' using {BaseUrl} (timeout: {Timeout}s, max retries: {MaxRetries})", 
            artistName, releaseTitle, baseUrl, options.Value.TimeoutSeconds, options.Value.MaxRetries);

        // Test basic connectivity first
        var isConnected = await TestConnectivityAsync(cancellationToken);
        if (!isConnected)
        {
            logger.LogWarning("[Prowlarr] Cannot connect to Prowlarr server at {BaseUrl}. Skipping search.", baseUrl);
            return Array.Empty<ProwlarrRelease>();
        }

        // Build search query with quality preferences
        var baseQuery = artistName + " " + releaseTitle;
        
        // Create multiple search variants with different quality priorities
        var searchQueries = new List<string>
        {
            // 1. Search for 320 kbps MP3 first (good quality, reasonable file size)
            baseQuery + " 320",
            // 2. Search for FLAC (lossless quality)
            baseQuery + " FLAC",
            // 3. Fallback to base query (no quality specified)
            baseQuery
        };
        
        logger.LogInformation("[Prowlarr] Quality-based search strategy for '{Artist} - {Release}': 320 → FLAC → no quality", artistName, releaseTitle);
        logger.LogInformation("[Prowlarr] Will try search queries in order: {Queries}", string.Join(" | ", searchQueries));
        
        // Try each search query until we find results, with retry logic
        foreach (var searchQuery in searchQueries)
        {
            logger.LogInformation("[Prowlarr] Trying quality level: '{Query}'", searchQuery);
            
            // Retry logic for transient failures
            var maxRetries = options.Value.MaxRetries;
            for (int attempt = 1; attempt <= maxRetries + 1; attempt++)
            {
                try
                {
                    var results = await TrySearchWithQueryAsync(searchQuery, artistName, releaseTitle, cancellationToken);
                    if (results.Count > 0)
                    {
                        var qualityLevel = searchQuery == baseQuery ? "no quality specified" : 
                                         searchQuery.Contains("320") ? "320 kbps" : 
                                         searchQuery.Contains("FLAC") ? "FLAC" : "unknown";
                        logger.LogInformation("[Prowlarr] Found {Count} results with {QualityLevel} search: '{Query}'", results.Count, qualityLevel, searchQuery);
                        return results;
                    }
                    else
                    {
                        logger.LogInformation("[Prowlarr] No results with '{Query}', trying next quality level", searchQuery);
                        break; // No results, try next quality level
                    }
                }
                catch (TaskCanceledException) when (attempt <= maxRetries)
                {
                    logger.LogWarning("[Prowlarr] Attempt {Attempt} timed out for '{Query}', retrying... ({Remaining} attempts remaining)", 
                        attempt, searchQuery, maxRetries - attempt + 1);
                    // Wait a bit before retrying
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    continue;
                }
                catch (HttpRequestException) when (attempt <= maxRetries)
                {
                    logger.LogWarning("[Prowlarr] Attempt {Attempt} failed with HTTP error for '{Query}', retrying... ({Remaining} attempts remaining)", 
                        attempt, searchQuery, maxRetries - attempt + 1);
                    // Wait a bit before retrying
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    continue;
                }
                catch (Exception ex) when (attempt <= maxRetries)
                {
                    logger.LogWarning(ex, "[Prowlarr] Attempt {Attempt} failed for '{Query}', retrying... ({Remaining} attempts remaining)", 
                        attempt, searchQuery, maxRetries - attempt + 1);
                    // Wait a bit before retrying
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    continue;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[Prowlarr] Final attempt failed for '{Query}' after {Attempt} attempts", searchQuery, attempt);
                    break; // Final attempt failed, try next quality level
                }
            }
        }
        
        logger.LogInformation("[Prowlarr] No results found with any quality level (320, FLAC, or no quality specified)");
        return Array.Empty<ProwlarrRelease>();
    }
    
    private async Task<IReadOnlyList<ProwlarrRelease>> TrySearchWithQueryAsync(string query, string artistName, string releaseTitle, CancellationToken cancellationToken)
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
                var list = ParseProwlarrResults(doc.RootElement, artistName, releaseTitle, logger);
                logger.LogInformation("[Prowlarr] Parsed {Count} results from GET", list.Count);
                if (list.Count == 0)
                {
                    logger.LogInformation("[Prowlarr] No results from this GET variant; trying next, if any.");
                    try { LogRootShape(doc.RootElement, logger); } catch { }
                    continue;
                }
                return list;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TaskCanceledException)
            {
                logger.LogWarning(ex, "Prowlarr search timed out for {Url} - this may indicate network issues or server unresponsiveness", url);
                continue;
            }
            catch (TaskCanceledException ex)
            {
                logger.LogWarning(ex, "Prowlarr search was canceled for {Url}", url);
                continue;
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
        // Try a different search approach - the POST endpoint seems to be for grabbing specific releases
        // Let's try using the same endpoint but with different parameters or a different method
        try
        {
            var baseUrl2 = options.Value.BaseUrl!.TrimEnd('/');
            var apiKey2 = options.Value.ApiKey!;
            // Try using the search endpoint with query parameters instead of POST body
            var url = $"{baseUrl2}/api/v1/search?apikey={Uri.EscapeDataString(apiKey2)}&query={Uri.EscapeDataString(query)}&type=search";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Accept.Clear();
            req.Headers.Accept.ParseAdd("application/json");
            try { req.Headers.Add("X-Api-Key", apiKey2); } catch { }
            
            logger.LogInformation("[Prowlarr] Alternative GET {Url}", url);
            var resp = await httpClient.SendAsync(req, cancellationToken);
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync(cancellationToken);
                var preview = json.Length > 600 ? json[..600] + "…" : json;
                logger.LogInformation("[Prowlarr] Alternative GET body preview: {Preview}", preview);
                using var doc = JsonDocument.Parse(json);
                var list = ParseProwlarrResults(doc.RootElement, artistName, releaseTitle, logger);
                logger.LogInformation("[Prowlarr] Parsed {Count} results from alternative GET", list.Count);
                if (list.Count > 0)
                {
                    return list;
                }
            }
            else
            {
                string? body = null;
                try { body = await resp.Content.ReadAsStringAsync(cancellationToken); } catch { }
                if (!string.IsNullOrWhiteSpace(body) && body.Length > 500) body = body[..500] + "…";
                logger.LogInformation("[Prowlarr] Alternative GET HTTP {Status}. Body: {Body}", (int)resp.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Prowlarr alternative search failed");
        }
        logger.LogWarning("Prowlarr search failed for all URL variants for query '{Query}'", query);
        return Array.Empty<ProwlarrRelease>();
    }

    private static List<ProwlarrRelease> ParseProwlarrResults(JsonElement root, string artistName, string releaseTitle, ILogger logger)
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
        var totalResults = 0;
        var filteredResults = 0;
        
        foreach (var item in array.EnumerateArray())
        {
            totalResults++;
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
            
            var release = new ProwlarrRelease(title, guid, magnet, downloadUrl, size, indexerId);
            
            // Validate and score the result
            if (IsValidMusicResult(release, artistName, releaseTitle))
            {
                list.Add(release);
            }
            else
            {
                filteredResults++;
                var reason = GetRejectionReason(release, artistName, releaseTitle);
                logger.LogDebug("[Prowlarr] Filtered out result '{Title}': {Reason}", title, reason);
            }
        }
        
        logger.LogInformation("[Prowlarr] Filtered {Filtered}/{Total} results for '{Artist} - {Album}'", 
            filteredResults, totalResults, artistName, releaseTitle);
        
        // Sort by relevance score (highest first)
        list.Sort((a, b) => CalculateRelevanceScore(b, artistName, releaseTitle).CompareTo(CalculateRelevanceScore(a, artistName, releaseTitle)));
        
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
    
    private static bool IsValidMusicResult(ProwlarrRelease release, string artistName, string releaseTitle)
    {
        if (string.IsNullOrWhiteSpace(release.Title))
            return false;
            
        var title = release.Title.ToLowerInvariant();
        var artist = artistName.ToLowerInvariant();
        var album = releaseTitle.ToLowerInvariant();
        
        // Reject obvious non-music content
        if (ContainsNonMusicTerms(title))
            return false;
            
        // Must contain artist name (or close variation)
        if (!ContainsArtistName(title, artist))
            return false;
            
        // Must contain album title (or close variation)
        if (!ContainsAlbumTitle(title, album))
            return false;
            
        // Reject torrent files for SABnzbd
        if (IsTorrentFile(release.DownloadUrl))
            return false;
            
        return true;
    }
    
    private static bool ContainsNonMusicTerms(string title)
    {
        var nonMusicTerms = new[]
        {
            "1080p", "720p", "4k", "hdtv", "web-dl", "bluray", "dvdrip", "h264", "h265", "x264", "x265",
            "season", "episode", "s01", "s02", "e01", "e02", "complete", "series",
            "movie", "film", "documentary", "show", "tv", "television",
            "subtitle", "dub", "dubbed", "multi", "dual", "audio"
        };
        
        return nonMusicTerms.Any(term => title.Contains(term));
    }
    
    private static bool ContainsArtistName(string title, string artistName)
    {
        // Split artist name into words and check if most are present
        var artistWords = artistName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleWords = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        // At least 50% of artist words must be present
        var matchingWords = artistWords.Count(word => titleWords.Any(titleWord => 
            titleWord.Contains(word) || word.Contains(titleWord)));
            
        return matchingWords >= Math.Max(1, artistWords.Length / 2);
    }
    
    private static bool ContainsAlbumTitle(string title, string albumTitle)
    {
        // Split album title into words and check if most are present
        var albumWords = albumTitle.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleWords = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        // At least 50% of album words must be present
        var matchingWords = albumWords.Count(word => titleWords.Any(titleWord => 
            titleWord.Contains(word) || word.Contains(titleWord)));
            
        return matchingWords >= Math.Max(1, albumWords.Length / 2);
    }
    
    private static bool IsTorrentFile(string? downloadUrl)
    {
        if (string.IsNullOrWhiteSpace(downloadUrl))
            return false;
            
        return downloadUrl.EndsWith(".torrent", StringComparison.OrdinalIgnoreCase) ||
               downloadUrl.Contains("torrent", StringComparison.OrdinalIgnoreCase);
    }
    
    private static int CalculateRelevanceScore(ProwlarrRelease release, string artistName, string releaseTitle)
    {
        if (string.IsNullOrWhiteSpace(release.Title))
            return 0;
            
        var title = release.Title.ToLowerInvariant();
        var artist = artistName.ToLowerInvariant();
        var album = releaseTitle.ToLowerInvariant();
        
        int score = 0;
        
        // Base score for being valid
        score += 10;
        
        // Bonus for exact artist name match
        if (title.Contains(artist))
            score += 20;
            
        // Bonus for exact album title match
        if (title.Contains(album))
            score += 20;
            
        // Bonus for quality indicators
        var qualityTerms = new[] { "flac", "lossless", "320", "cd", "vinyl", "24bit", "16bit" };
        foreach (var term in qualityTerms)
        {
            if (title.Contains(term))
                score += 5;
        }
        
        // Bonus for common music file extensions
        var musicExtensions = new[] { ".mp3", ".flac", ".m4a", ".wav", ".ogg", ".aac" };
        foreach (var ext in musicExtensions)
        {
            if (title.Contains(ext))
                score += 3;
        }
        
        // Penalty for torrent files
        if (IsTorrentFile(release.DownloadUrl))
            score -= 50;
            
        return score;
    }
    
    private static string GetRejectionReason(ProwlarrRelease release, string artistName, string releaseTitle)
    {
        if (string.IsNullOrWhiteSpace(release.Title))
            return "Missing title";
            
        var title = release.Title.ToLowerInvariant();
        var artist = artistName.ToLowerInvariant();
        var album = releaseTitle.ToLowerInvariant();
        
        // Check each validation step
        if (ContainsNonMusicTerms(title))
            return "Contains non-music terms (TV/movie/video)";
            
        if (!ContainsArtistName(title, artist))
            return "Artist name mismatch";
            
        if (!ContainsAlbumTitle(title, album))
            return "Album title mismatch";
            
        if (IsTorrentFile(release.DownloadUrl))
            return "Torrent file (not compatible with SABnzbd)";
            
        return "Unknown validation failure";
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


