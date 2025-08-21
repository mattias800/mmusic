using Microsoft.Extensions.Options;
using MusicGQL.Features.External.Downloads.QBittorrent;
using MusicGQL.Features.External.Downloads.Sabnzbd;
using MusicGQL.Features.External.Downloads.Prowlarr.Configuration;

namespace MusicGQL.Features.External.Downloads.Prowlarr;

public class ProwlarrDownloadProvider(
    ProwlarrClient prowlarr,
    QBittorrentClient qb,
    SabnzbdClient sab,
    IOptions<ProwlarrOptions> prowlarrOptions,
    MusicGQL.Features.ServerSettings.ServerSettingsAccessor serverSettingsAccessor,
    ILogger<ProwlarrDownloadProvider> logger
) : IDownloadProvider
{
    public async Task<bool> TryDownloadReleaseAsync(
        string artistId,
        string releaseFolderName,
        string artistName,
        string releaseTitle,
        string targetDirectory,
        IReadOnlyList<int> allowedOfficialCounts,
        IReadOnlyList<int> allowedOfficialDigitalCounts,
        CancellationToken cancellationToken
    )
    {
        // Read toggles
        var settings = await serverSettingsAccessor.GetAsync();
        var allowSab = settings.EnableSabnzbdDownloader;
        var allowQbit = settings.EnableQBittorrentDownloader;

        // 1) Search Prowlarr for nzb/magnet results
        logger.LogInformation("[Prowlarr] Begin provider for {Artist} - {Release}", artistName, releaseTitle);
        var results = await prowlarr.SearchAlbumAsync(artistName, releaseTitle, cancellationToken);
        logger.LogInformation("[Prowlarr] Search returned {Count} results for {Artist} - {Release}", results.Count, artistName, releaseTitle);
        // Prefer items that look like full releases and larger sizes first
        // Filter and score results for music albums only; reject obvious mismatches (e.g., TV packs)
        results = results
            .Where(r => IsLikelyMusicAlbum(r, artistName, releaseTitle))
            .OrderByDescending(r => Score(r, artistName, releaseTitle))
            .ThenByDescending(r => r.Size ?? 0)
            .ToList();
        if (results.Count == 0) return false;

        // 2) Prefer NZB if available; otherwise fallback to magnet
        // Prefer Prowlarr-provided downloadUrl; many setups do not include .nzb extension
        // Extra guard: only consider results whose title clearly matches artist and release
        bool TitleMatches(string? title)
        {
            if (string.IsNullOrWhiteSpace(title)) return false;
            var t = title.ToLowerInvariant();
            var a = (artistName ?? string.Empty).ToLowerInvariant();
            var rel = (releaseTitle ?? string.Empty).ToLowerInvariant();
            // Require both artist and at least half of album words
            if (!t.Contains(a)) return false;
            var albumWords = rel.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var titleWords = t.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var matchCount = albumWords.Count(w => titleWords.Any(x => x.Contains(w) || w.Contains(x)));
            return matchCount >= Math.Max(1, albumWords.Length / 2);
        }

        // Prefer NZB for SAB, but explicitly avoid .torrent URLs here
        var nzb = results.FirstOrDefault(r =>
            !string.IsNullOrWhiteSpace(r.DownloadUrl)
            && !LooksLikeTorrentUrl(r.DownloadUrl!)
            && TitleMatches(r.Title)
        );
        if (allowSab && nzb is not null && !string.IsNullOrWhiteSpace(nzb.DownloadUrl))
        {
            // Always fetch NZB bytes and upload them to SABnzbd to avoid SAB needing network access to Prowlarr
            var url = EnsureProwlarrApiKey(nzb.DownloadUrl!);
            logger.LogInformation("[Prowlarr] Uploading NZB to SABnzbd: {Title}", nzb.Title ?? "(no title)");
            try
            {
                using var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync(url, cancellationToken);
                // Validate the payload looks like an NZB, not a torrent
                if (!LooksLikeNzb(bytes))
                {
                    logger.LogWarning("[Prowlarr] Fetched content does not look like NZB, skipping upload (likely torrent)");
                    goto TryMagnet;
                }
                var fileName = (nzb.Title ?? "download").Replace(' ', '+') + ".nzb";
                var okUpload = await sab.AddNzbByContentAsync(
                    bytes,
                    fileName,
                    cancellationToken,
                    pathOverride: $"mmusic/{artistId}/{releaseFolderName}",
                    nzbName: $"{artistName} - {releaseTitle}"
                );
                if (okUpload) return true;
                logger.LogWarning("[Prowlarr] SABnzbd rejected NZB content upload");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[Prowlarr] Failed fetching/uploading NZB content");
            }
        }

        TryMagnet:
        var magnet = results.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.MagnetUrl));
        if (allowQbit && magnet is not null && !string.IsNullOrWhiteSpace(magnet.MagnetUrl))
        {
            logger.LogInformation("[Prowlarr] Handing off magnet to qBittorrent: {Title}", magnet.Title ?? "(no title)");
            var ok = await qb.AddMagnetAsync(magnet.MagnetUrl!, null, cancellationToken);
            if (ok) return true;
            logger.LogWarning("[Prowlarr] qBittorrent did not accept magnet handoff");
        }

        // Fallbacks by type: if we have a torrent URL, send to qBittorrent; if generic HTTP and NZB-like, send to SAB
        var torrentUrl = results.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.DownloadUrl) && LooksLikeTorrentUrl(r.DownloadUrl!))?.DownloadUrl;
        if (allowQbit && !string.IsNullOrWhiteSpace(torrentUrl))
        {
            logger.LogInformation("[Prowlarr] Handing off .torrent URL to qBittorrent");
            var okT = await qb.AddByUrlAsync(EnsureProwlarrApiKey(torrentUrl!), null, cancellationToken);
            if (okT) return true;
        }

        var httpUrl = results.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.DownloadUrl))?.DownloadUrl;
        if (allowSab && !string.IsNullOrWhiteSpace(httpUrl) && (httpUrl!.StartsWith("http://") || httpUrl.StartsWith("https://")))
        {
            var url2 = EnsureProwlarrApiKey(httpUrl);
            logger.LogInformation("[Prowlarr] Attempting SABnzbd handoff using HTTP URL: {Url}", url2);
            var ok2 = await sab.AddNzbByUrlAsync(url2, $"{artistName} - {releaseTitle}", cancellationToken);
            if (ok2) return true;
        }

        // Diagnostic: log first item details when present
        try
        {
            var first = results.FirstOrDefault();
            if (first != null)
            {
                logger.LogInformation("[Prowlarr] First result details: title={Title}, indexerId={IndexerId}, guid={Guid}, downloadUrl={DownloadUrl}, hasMagnet={HasMagnet}",
                    first.Title ?? "", first.IndexerId, first.Guid ?? "", first.DownloadUrl ?? "", !string.IsNullOrWhiteSpace(first.MagnetUrl));
            }
        }
        catch { }

        logger.LogInformation("Prowlarr provider found results but failed to hand off to downloader");
        return false;
    }

    private string EnsureProwlarrApiKey(string downloadUrl)
    {
        try
        {
            var baseUrl = prowlarrOptions.Value.BaseUrl?.TrimEnd('/');
            var apiKey = prowlarrOptions.Value.ApiKey;
            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey)) return downloadUrl;

            // If the download url points to prowlarr and lacks apikey, append it
            if (downloadUrl.StartsWith("/"))
            {
                downloadUrl = baseUrl + downloadUrl;
            }
            if (downloadUrl.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase) && downloadUrl.IndexOf("apikey=", StringComparison.OrdinalIgnoreCase) < 0)
            {
                var sep = downloadUrl.Contains('?') ? '&' : '?';
                return downloadUrl + sep + "apikey=" + Uri.EscapeDataString(apiKey);
            }
            return downloadUrl;
        }
        catch { return downloadUrl; }
    }

    private static bool LooksLikeNzb(byte[] bytes)
    {
        try
        {
            // Check for ASCII XML signature and <nzb> tag in the first few KB
            var sampleLen = Math.Min(bytes.Length, 4096);
            var sample = System.Text.Encoding.UTF8.GetString(bytes, 0, sampleLen);
            if (sample.IndexOf("<nzb", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            // Common torrent indicator near start
            if (sample.IndexOf("announce", StringComparison.OrdinalIgnoreCase) >= 0) return false;
            // Fallback: looks like XML
            if (sample.TrimStart().StartsWith("<?xml")) return true;
        }
        catch { }
        return false;
    }

    private static bool LooksLikeTorrentUrl(string downloadUrl)
    {
        try
        {
            return downloadUrl.EndsWith(".torrent", StringComparison.OrdinalIgnoreCase)
                   || downloadUrl.Contains("torrent", StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    private static bool IsLikelyMusicAlbum(ProwlarrRelease r, string artistName, string releaseTitle)
    {
        var title = (r.Title ?? string.Empty).ToLowerInvariant();
        // Reject obvious non-music signals
        if (title.Contains("s01") || title.Contains("s1 ") || title.Contains(" s11 ") || title.Contains("season ") || title.Contains("s11-s") || title.Contains("e01") || title.Contains("1080p") || title.Contains("2160p"))
        {
            // Allow resolution presence for music only if artist and release tokens both match strongly
            if (Score(r, artistName, releaseTitle) < 5) return false;
        }
        if (title.Contains("simpsons") || title.Contains("spongebob") || title.Contains("season") || title.Contains("episode")) return false;
        if (title.Contains("x264") || title.Contains("x265") || title.Contains("h264") || title.Contains("h265"))
        {
            if (Score(r, artistName, releaseTitle) < 6) return false;
        }
        // Prefer music-specific tags
        if (title.Contains("flac") || title.Contains("mp3") || title.Contains("lossless") || title.Contains("album") || title.Contains("disc "))
        {
            return true;
        }
        // Fallback: require both artist and release tokens
        return Score(r, artistName, releaseTitle) >= 4;
    }

    private static int Score(ProwlarrRelease r, string artistName, string releaseTitle)
    {
        static string Norm(string s)
        {
            s = s.Replace("’", "'").ToLowerInvariant();
            var keep = new System.Text.StringBuilder(s.Length);
            foreach (var ch in s)
            {
                if (char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch)) keep.Append(ch);
            }
            return System.Text.RegularExpressions.Regex.Replace(keep.ToString(), "\\s+", " ").Trim();
        }
        var title = Norm(r.Title ?? string.Empty);
        var artist = Norm(artistName);
        var rel = Norm(releaseTitle);
        int score = 0;
        if (!string.IsNullOrWhiteSpace(artist) && title.Contains(artist)) score += Math.Min(artist.Length, 5);
        if (!string.IsNullOrWhiteSpace(rel) && title.Contains(rel)) score += Math.Min(rel.Length, 5);
        // Light bonus for music tags
        if (title.Contains("flac")) score += 2;
        if (title.Contains("mp3")) score += 1;
        if (title.Contains("lossless")) score += 2;
        if (title.Contains("album")) score += 1;
        return score;
    }
}


