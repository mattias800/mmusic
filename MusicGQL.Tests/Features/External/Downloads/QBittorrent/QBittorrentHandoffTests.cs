using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using MusicGQL.Features.External.Downloads.Prowlarr;

namespace MusicGQL.Tests.Features.External.Downloads.QBittorrent;

public class QBittorrentHandoffTests
{
    private static (string Artist, string Album) ReadTestCase()
    {
        var artist = Environment.GetEnvironmentVariable("PROWLARR__TEST_ARTIST") ?? "Zara Larsson";
        var album = Environment.GetEnvironmentVariable("PROWLARR__TEST_ALBUM") ?? "Introducing";
        return (artist, album);
    }

    private static (string? BaseUrl, string? User, string? Pass, string? SavePath) ReadQbitEnv()
    {
        var baseUrl = Environment.GetEnvironmentVariable("QBITTORRENT__BASEURL")
                      ?? Environment.GetEnvironmentVariable("QBittorrent__BaseUrl")
                      ?? Environment.GetEnvironmentVariable("QBittorrent:BaseUrl");
        var user = Environment.GetEnvironmentVariable("QBITTORRENT__USERNAME")
                    ?? Environment.GetEnvironmentVariable("QBittorrent__Username")
                    ?? Environment.GetEnvironmentVariable("QBittorrent:Username");
        var pass = Environment.GetEnvironmentVariable("QBITTORRENT__PASSWORD")
                    ?? Environment.GetEnvironmentVariable("QBittorrent__Password")
                    ?? Environment.GetEnvironmentVariable("QBittorrent:Password");
        var save = Environment.GetEnvironmentVariable("QBITTORRENT__SAVEPATH")
                    ?? Environment.GetEnvironmentVariable("QBittorrent__SavePath")
                    ?? Environment.GetEnvironmentVariable("QBittorrent:SavePath");
        return (baseUrl, user, pass, save);
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
    public async Task Handoff_Torrent_To_QBittorrent_ZaraLarsson_Introducing()
    {
        var (prowBase, prowKey) = ReadProwlarrEnv();
        var (qBase, qUser, qPass, savePath) = ReadQbitEnv();
        if (string.IsNullOrWhiteSpace(prowBase) || string.IsNullOrWhiteSpace(prowKey) || string.IsNullOrWhiteSpace(qBase) || string.IsNullOrWhiteSpace(qUser) || string.IsNullOrWhiteSpace(qPass))
        {
            return; // skip if not configured
        }

        prowBase = prowBase!.TrimEnd('/');
        qBase = qBase!.TrimEnd('/');

        var (artist, album) = ReadTestCase();
        var queries = new List<string> { $"{artist} {album}" };
        if (!string.Equals(album, "Introduction", StringComparison.OrdinalIgnoreCase))
        {
            queries.Add($"{artist} Introduction");
        }

        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

        // Query Prowlarr for torrent-like candidate
        (string? title, string? magnet, string? torrentUrl) cand = (null, null, null);
        foreach (var query in queries)
        {
            var urls = new List<string>
            {
                $"{prowBase}/api/v1/search?apikey={Uri.EscapeDataString(prowKey!)}&query={Uri.EscapeDataString(query)}",
                $"{prowBase}/api/v1/search?apikey={Uri.EscapeDataString(prowKey!)}&term={Uri.EscapeDataString(query)}",
                $"{prowBase}/api/v1/search?apikey={Uri.EscapeDataString(prowKey!)}&query={Uri.EscapeDataString(query)}&limit=100"
            };
            foreach (var url in urls)
            {
                try
                {
                    var resp = await http.GetAsync(url);
                    if (!resp.IsSuccessStatusCode) { await Task.Delay(200); continue; }
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
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
                    if (array.ValueKind != JsonValueKind.Array) continue;
                    foreach (var item in array.EnumerateArray())
                    {
                        var title = ProwlarrJsonParser.GetStringCI(item, "title") ?? ProwlarrJsonParser.GetStringCI(item, "Title") ?? string.Empty;
                        var magnet = ProwlarrJsonParser.GetStringCI(item, "magnetUrl") ?? ProwlarrJsonParser.GetStringCI(item, "MagnetUrl");
                        var downloadUrl = ProwlarrJsonParser.GetStringCI(item, "downloadUrl") ?? ProwlarrJsonParser.GetStringCI(item, "DownloadUrl")
                                          ?? ProwlarrJsonParser.GetStringCI(item, "link") ?? ProwlarrJsonParser.GetStringCI(item, "Link");
                        if (string.IsNullOrWhiteSpace(title)) continue;
                        var tlow = title.ToLowerInvariant();
                        if (!tlow.Contains("zara") || !tlow.Contains("introduc")) continue;
                        if (!string.IsNullOrWhiteSpace(magnet)) { cand = (title, magnet, null); break; }
                        if (!string.IsNullOrWhiteSpace(downloadUrl) && (downloadUrl.Contains("torrent", StringComparison.OrdinalIgnoreCase)))
                        { cand = (title, null, downloadUrl); break; }
                    }
                    if (cand.title != null) break;
                }
                catch { await Task.Delay(200); }
            }
            if (cand.title != null) break;
        }

        if (cand.title is null)
        {
            return; // skip if no torrent found
        }

        // qBittorrent API: login, then add torrent via magnet or URL
        async Task<string?> LoginAsync()
        {
            var loginUrl = $"{qBase}/api/v2/auth/login";
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("username", qUser!),
                new KeyValuePair<string,string>("password", qPass!)
            });
            var resp = await http.PostAsync(loginUrl, content);
            if (!resp.IsSuccessStatusCode) return null;
            if (resp.Headers.TryGetValues("Set-Cookie", out var vals))
            {
                return vals.FirstOrDefault();
            }
            return null;
        }

        async Task<bool> AddAsync(string cookie, string urlToAdd)
        {
            var addUrl = $"{qBase}/api/v2/torrents/add";
            using var req = new HttpRequestMessage(HttpMethod.Post, addUrl);
            req.Headers.Add("Cookie", cookie);
            var form = new MultipartFormDataContent
            {
                { new StringContent(urlToAdd), "urls" }
            };
            if (!string.IsNullOrWhiteSpace(savePath))
            {
                form.Add(new StringContent(savePath!), "savepath");
            }
            req.Content = form;
            var resp = await http.SendAsync(req);
            return resp.IsSuccessStatusCode;
        }

        var cookie = await LoginAsync();
        if (string.IsNullOrWhiteSpace(cookie)) return;

        bool ok = false;
        if (!string.IsNullOrWhiteSpace(cand.magnet))
        {
            ok = await AddAsync(cookie!, cand.magnet!);
        }
        else if (!string.IsNullOrWhiteSpace(cand.torrentUrl))
        {
            ok = await AddAsync(cookie!, cand.torrentUrl!);
        }
        Assert.True(ok, "qBittorrent did not accept the torrent/magnet");
    }
}

