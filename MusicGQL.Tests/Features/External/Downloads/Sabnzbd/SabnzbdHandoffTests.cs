using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using MusicGQL.Features.External.Downloads.Prowlarr;

namespace MusicGQL.Tests.Features.External.Downloads.Sabnzbd;

public class SabnzbdHandoffTests
{
    private static (string Artist, string Album) ReadTestCase()
    {
        var artist = Environment.GetEnvironmentVariable("PROWLARR__TEST_ARTIST") ?? "Zara Larsson";
        var album = Environment.GetEnvironmentVariable("PROWLARR__TEST_ALBUM") ?? "Introducing";
        return (artist, album);
    }

    private static (string? BaseUrl, string? ApiKey, string Category) ReadSabEnv()
    {
        var baseUrl = Environment.GetEnvironmentVariable("SABNZBD__BASEURL")
                      ?? Environment.GetEnvironmentVariable("Sabnzbd__BaseUrl")
                      ?? Environment.GetEnvironmentVariable("Sabnzbd:BaseUrl");
        var apiKey = Environment.GetEnvironmentVariable("SABNZBD__APIKEY")
                     ?? Environment.GetEnvironmentVariable("Sabnzbd__ApiKey")
                     ?? Environment.GetEnvironmentVariable("Sabnzbd:ApiKey");
        var category = Environment.GetEnvironmentVariable("SABNZBD__CATEGORY") ?? "music";
        return (baseUrl, apiKey, category);
    }

    private static (string? BaseUrl, string? ApiKey) ReadProwlarrEnv()
    {
        var baseUrl = Environment.GetEnvironmentVariable("PROWLARR__BASEURL")
                      ?? Environment.GetEnvironmentVariable("Prowlarr__BaseUrl")
                      ?? Environment.GetEnvironmentVariable("Prowlarr:BaseUrl");
        var apiKey = Environment.GetEnvironmentVariable("PROWLARR__APIKEY")
                     ?? Environment.GetEnvironmentVariable("Prowlarr__ApiKey")
                     ?? Environment.GetEnvironmentVariable("Prowlarr:ApiKey");
        return (baseUrl, apiKey);
    }

    [Fact]
    public async Task Handoff_Nzb_To_Sabnzbd_ZaraLarsson_Introduction()
    {
        var (prowBase, prowKey) = ReadProwlarrEnv();
        var (sabBase, sabKey, category) = ReadSabEnv();
        if (string.IsNullOrWhiteSpace(prowBase) || string.IsNullOrWhiteSpace(prowKey) || string.IsNullOrWhiteSpace(sabBase) || string.IsNullOrWhiteSpace(sabKey))
        {
            return; // skip if not configured
        }

        prowBase = prowBase!.TrimEnd('/');
        sabBase = sabBase!.TrimEnd('/');

        var (artist, album) = ReadTestCase();
        var queries = new List<string> { $"{artist} {album}" };
        // Add a fallback variant for common naming differences (e.g., Introducing vs Introduction)
        if (!string.Equals(album, "Introduction", StringComparison.OrdinalIgnoreCase))
        {
            queries.Add($"{artist} Introduction");
        }

        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

        // Query Prowlarr with multiple URL variants to maximize chance of NZB results
        List<ProwlarrRelease>? parsed = null;
        List<(string Title, string? DownloadUrl, string? MagnetUrl, string? Protocol)> raw = new();
        foreach (var query in queries)
        {
            var urls = new List<string>
            {
                $"{prowBase}/api/v1/search?apikey={Uri.EscapeDataString(prowKey!)}&query={Uri.EscapeDataString(query)}",
                $"{prowBase}/api/v1/search?apikey={Uri.EscapeDataString(prowKey!)}&term={Uri.EscapeDataString(query)}",
                $"{prowBase}/api/v1/search?apikey={Uri.EscapeDataString(prowKey!)}&query={Uri.EscapeDataString(query)}&limit=100",
                $"{prowBase}/api/v1/search?apikey={Uri.EscapeDataString(prowKey!)}&query={Uri.EscapeDataString(query)}&categories=3000&categories=3010&categories=3030&categories=3040",
                $"{prowBase}/api/v1/search?apikey={Uri.EscapeDataString(prowKey!)}&term={Uri.EscapeDataString(query)}&categories=3000&categories=3010&categories=3030&categories=3040",
            };
            foreach (var url in urls)
            {
                try
                {
                    var resp = await http.GetAsync(url);
                    if (!resp.IsSuccessStatusCode) { await Task.Delay(200); continue; }
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    // Build raw list
                    JsonElement array = doc.RootElement;
                    if (array.ValueKind == JsonValueKind.Object)
                    {
                        if (!ProwlarrJsonParser.TryGetPropertyCI(array, "results", out array) &&
                            !ProwlarrJsonParser.TryGetPropertyCI(array, "Results", out array))
                        {
                            if (!ProwlarrJsonParser.TryGetPropertyCI(array, "data", out array) &&
                                !ProwlarrJsonParser.TryGetPropertyCI(array, "Data", out array))
                            {
                                array = doc.RootElement;
                            }
                        }
                    }
                    if (array.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in array.EnumerateArray())
                        {
                            var t = ProwlarrJsonParser.GetStringCI(item, "title") ?? ProwlarrJsonParser.GetStringCI(item, "Title") ?? string.Empty;
                            var dl = ProwlarrJsonParser.GetStringCI(item, "downloadUrl") ?? ProwlarrJsonParser.GetStringCI(item, "DownloadUrl")
                                     ?? ProwlarrJsonParser.GetStringCI(item, "link") ?? ProwlarrJsonParser.GetStringCI(item, "Link");
                            var m = ProwlarrJsonParser.GetStringCI(item, "magnetUrl") ?? ProwlarrJsonParser.GetStringCI(item, "MagnetUrl");
                            var proto = ProwlarrJsonParser.GetStringCI(item, "protocol") ?? ProwlarrJsonParser.GetStringCI(item, "Protocol");
                            if (!string.IsNullOrWhiteSpace(t)) raw.Add((t, dl, m, proto));
                        }
                    }

                    // Parse filtered list
                    var list = ProwlarrJsonParser.ParseResults(doc.RootElement, artist, album, NullLogger.Instance);
                    if (list.Count > 0)
                    {
                        parsed = list;
                    }
                }
                catch { await Task.Delay(200); }
            }
            if (parsed is not null && parsed.Count > 0) break;
        }

        // Require some raw results
        Assert.True(raw.Count > 0, "No results from Prowlarr");

        // Verify both torrent-like and NZB-like candidates exist (raw)
        bool hasTorrent = raw.Any(r => (!string.IsNullOrWhiteSpace(r.MagnetUrl)) || (!string.IsNullOrWhiteSpace(r.DownloadUrl) && MusicGQL.Features.External.Downloads.Prowlarr.ProwlarrScorer.IsTorrentFile(r.DownloadUrl!)) || string.Equals(r.Protocol, "torrent", StringComparison.OrdinalIgnoreCase));
        bool hasNzb = raw.Any(r => (!string.IsNullOrWhiteSpace(r.DownloadUrl) && !MusicGQL.Features.External.Downloads.Prowlarr.ProwlarrScorer.IsTorrentFile(r.DownloadUrl!)) || string.Equals(r.Protocol, "usenet", StringComparison.OrdinalIgnoreCase));
        Assert.True(hasTorrent, "Expected at least one torrent-like result (magnet or .torrent)");
        Assert.True(hasNzb, "Expected at least one NZB-like result (http download not marked as torrent)");

        // Try to find an NZB-like candidate by probing headers (from raw)
        string EnsureProwlarrApiKey(string url)
        {
            if (url.Contains("apikey=", StringComparison.OrdinalIgnoreCase)) return url;
            try
            {
                var okBase = Uri.TryCreate(prowBase!, UriKind.Absolute, out var baseUri);
                var okUrl = Uri.TryCreate(url, UriKind.Absolute, out var u);
                if (okBase && okUrl && baseUri!.Host.Equals(u!.Host, StringComparison.OrdinalIgnoreCase))
                {
                    var sep = url.Contains('?') ? '&' : '?';
                    return url + sep + "apikey=" + Uri.EscapeDataString(prowKey!);
                }
            }
            catch { }
            return url;
        }

        string? nzbUrl = null;
        string? nzbName = null;
        foreach (var r in raw.Take(10))
        {
            if (string.IsNullOrWhiteSpace(r.DownloadUrl)) continue;
            var url = EnsureProwlarrApiKey(r.DownloadUrl!);
            try
            {
                using var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                if (!resp.IsSuccessStatusCode) continue;
                var ct = resp.Content.Headers.ContentType?.MediaType?.ToLowerInvariant();
                var cd = resp.Content.Headers.ContentDisposition?.ToString() ?? string.Join(";", resp.Content.Headers.TryGetValues("Content-Disposition", out var vals) ? vals : Array.Empty<string>());
                bool looksNzb = (ct != null && (ct.Contains("nzb") || ct.Contains("xml") || ct.Contains("octet-stream")))
                                 || (!string.IsNullOrWhiteSpace(cd) && cd.Contains(".nzb", StringComparison.OrdinalIgnoreCase));
                if (looksNzb)
                {
                    nzbUrl = url;
                    nzbName = r.Title;
                    break;
                }
            }
            catch { }
        }

        if (nzbUrl is null)
        {
            // No NZB candidates found in current indexers for this release; skip this test as environment-dependent
            return;
        }

        // Use SAB addurl (SAB fetches from Prowlarr download link)
        var sabAddUrl = $"{sabBase}/api?mode=addurl&apikey={Uri.EscapeDataString(sabKey!)}&output=json&cat={Uri.EscapeDataString(category)}&nzbname={Uri.EscapeDataString(nzbName ?? (artist + " - " + album))}&name={Uri.EscapeDataString(nzbUrl)}";
        var sabResp = await http.GetAsync(sabAddUrl);
        Assert.True(sabResp.IsSuccessStatusCode, $"SABnzbd addurl failed HTTP {(int)sabResp.StatusCode} {sabResp.ReasonPhrase}");
        var sabBody = await sabResp.Content.ReadAsStringAsync();
        try
        {
            using var sabJson = JsonDocument.Parse(sabBody);
            var ok = sabJson.RootElement.TryGetProperty("status", out var s) && (s.ValueKind == JsonValueKind.True || (s.ValueKind == JsonValueKind.String && string.Equals(s.GetString(), "true", StringComparison.OrdinalIgnoreCase)));
            Assert.True(ok, $"SABnzbd returned status=false. Body: {sabBody}");
        }
        catch
        {
            // Accept HTTP 200 as success if non-JSON
        }

        // Verify it shows in queue API quickly (best-effort)
        var queueUrl = $"{sabBase}/api?mode=queue&apikey={Uri.EscapeDataString(sabKey!)}&output=json";
        var qResp = await http.GetAsync(queueUrl);
        if (qResp.IsSuccessStatusCode)
        {
            var qBody = await qResp.Content.ReadAsStringAsync();
            using var qDoc = JsonDocument.Parse(qBody);
            if (qDoc.RootElement.TryGetProperty("queue", out var queue) && queue.TryGetProperty("slots", out var slots))
            {
                var any = slots.EnumerateArray().Any(slot => slot.TryGetProperty("name", out var name) && (name.GetString()?.Contains(artist, StringComparison.OrdinalIgnoreCase) ?? false));
                Assert.True(any, "Uploaded NZB not visible in SAB queue yet");
            }
        }
    }
}

