using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using MusicGQL.Features.External.Downloads.Prowlarr;

namespace MusicGQL.Tests.Features.External.Downloads.Prowlarr;

public class ProwlarrLiveSearchTests
{
    private static (string Artist, string Album) ReadTestCase()
    {
        var artist = Environment.GetEnvironmentVariable("PROWLARR__TEST_ARTIST") ?? "Zara Larsson";
        var album = Environment.GetEnvironmentVariable("PROWLARR__TEST_ALBUM") ?? "Introducing";
        return (artist, album);
    }

    private static (string? BaseUrl, string? ApiKey) ReadConfig()
    {
        // Prefer environment variables: PROWLARR__BASEURL and PROWLARR__APIKEY
        var baseUrl = Environment.GetEnvironmentVariable("PROWLARR__BASEURL")
                      ?? Environment.GetEnvironmentVariable("Prowlarr__BaseUrl")
                      ?? Environment.GetEnvironmentVariable("Prowlarr:BaseUrl");
        var apiKey = Environment.GetEnvironmentVariable("PROWLARR__APIKEY")
                     ?? Environment.GetEnvironmentVariable("Prowlarr__ApiKey")
                     ?? Environment.GetEnvironmentVariable("Prowlarr:ApiKey");
        return (baseUrl, apiKey);
    }

    [Fact]
    public async Task Live_Search_ZaraLarsson_Introduction_SelectsCandidate()
    {
        var (baseUrl, apiKey) = ReadConfig();
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
        {
            // Skip: integration test requires PROWLARR__BASEURL and PROWLARR__APIKEY
            return;
        }

        baseUrl = baseUrl!.TrimEnd('/');

        var (artist, album) = ReadTestCase();
        var query = $"{artist} {album}";

        var urls = new List<string>
        {
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey!)}&query={Uri.EscapeDataString(query)}",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey!)}&query={Uri.EscapeDataString(query)}&type=search",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey!)}&query={Uri.EscapeDataString(query)}&limit=50",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey!)}&term={Uri.EscapeDataString(query)}",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey!)}&q={Uri.EscapeDataString(query)}",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey!)}&Query={Uri.EscapeDataString(query)}",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey!)}&query={Uri.EscapeDataString(query)}&categories=3000&categories=3010&categories=3030&categories=3040",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey!)}&term={Uri.EscapeDataString(query)}&categories=3000&categories=3010&categories=3030&categories=3040",
        };

        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        List<ProwlarrRelease>? parsed = null;
        foreach (var url in urls)
        {
            try
            {
                var resp = await http.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    await Task.Delay(300);
                    continue;
                }
                var json = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var list = ProwlarrJsonParser.ParseResults(doc.RootElement, artist, album, NullLogger.Instance);
                if (list.Count > 0)
                {
                    parsed = list;
                    break;
                }
            }
            catch
            {
                await Task.Delay(300);
            }
        }

        Assert.NotNull(parsed);
        Assert.True(parsed!.Count > 0, "No releases parsed from Prowlarr live search");

        // Decide with both downloaders allowed, discography disabled for single album correctness
        var selection = ProwlarrSelectionLogic.Decide(parsed!, artist, album, allowSab: true, allowQbit: true, discographyEnabled: false);
        Assert.NotEqual(ProwlarrSelectionType.None, selection.Type);
        Assert.NotNull(selection.Release);
        Assert.True(ProwlarrTextMatch.TitleMatches(selection.Release!.Title, artist, album), $"Selected title did not match: '{selection.Release!.Title}'");

        // Verify the selected candidate looks downloadable
        if (selection.Type is ProwlarrSelectionType.Nzb or ProwlarrSelectionType.Torrent)
        {
            Assert.NotNull(selection.UrlOrMagnet);
            var url = selection.UrlOrMagnet!;
            Assert.StartsWith("http", url, StringComparison.OrdinalIgnoreCase);

            // Try HEAD first to avoid full body download; fall back to GET headers-only
            try
            {
                using var headReq = new HttpRequestMessage(HttpMethod.Head, url);
                using var headResp = await http.SendAsync(headReq);
                if (!headResp.IsSuccessStatusCode)
                {
                    using var getResp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    Assert.True(getResp.IsSuccessStatusCode, $"Download URL not accessible: {(int)getResp.StatusCode} {getResp.ReasonPhrase}");
                }
            }
            catch (HttpRequestException ex)
            {
                Assert.Fail($"HTTP error verifying download URL: {ex.Message}");
            }
        }
        else if (selection.Type == ProwlarrSelectionType.Magnet)
        {
            Assert.NotNull(selection.UrlOrMagnet);
            var u = selection.UrlOrMagnet!;
            // Some indexers expose the magnet via an HTTP indirection URL
            if (u.StartsWith("magnet:", StringComparison.OrdinalIgnoreCase))
            {
                Assert.Contains("xt=urn:btih:", u, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                Assert.StartsWith("http", u, StringComparison.OrdinalIgnoreCase);
                // We avoid performing an HTTP request here because some indexers return a redirect
                // to a magnet URI in the Location header, which is not a valid HTTP URL.
                // Instead, validate the URL shape and host match the configured Prowlarr base.
                var ok = Uri.TryCreate(u, UriKind.Absolute, out var indirectionUri);
                Assert.True(ok && indirectionUri is not null, "Magnet indirection is not a valid absolute URL");
                var baseOk = Uri.TryCreate(baseUrl!, UriKind.Absolute, out var baseUri);
                Assert.True(baseOk && baseUri is not null, "BaseUrl is not a valid absolute URL");
                Assert.Equal(baseUri!.Host, indirectionUri!.Host);
                Assert.Contains("download", indirectionUri!.AbsolutePath, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}

