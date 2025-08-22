using Microsoft.Extensions.Options;
using MusicGQL.Features.External.Downloads.QBittorrent;
using MusicGQL.Features.External.Downloads.Sabnzbd;
using MusicGQL.Features.External.Downloads.Prowlarr.Configuration;
using MusicGQL.Features.Downloads.Services;

namespace MusicGQL.Features.External.Downloads.Prowlarr;

public class ProwlarrDownloadProvider(
    ProwlarrClient prowlarr,
    QBittorrentClient qb,
    SabnzbdClient sab,
    IOptions<ProwlarrOptions> prowlarrOptions,
    ServerSettings.ServerSettingsAccessor serverSettingsAccessor,
    ILogger<ProwlarrDownloadProvider> logger,
    DownloadLogPathProvider logPathProvider
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
        // Initialize per-release log
        DownloadLogger? relLoggerImpl = null;
        IDownloadLogger relLogger = new NullDownloadLogger();
        try
        {
            var logPath = await logPathProvider.GetReleaseLogFilePathAsync(artistName, releaseTitle, cancellationToken);
            if (!string.IsNullOrWhiteSpace(logPath))
            {
                relLoggerImpl = new DownloadLogger(logPath!);
                relLogger = relLoggerImpl;
            }
        }
        catch { }

        try
        {
            // Read toggles
            var settings = await serverSettingsAccessor.GetAsync();
        var allowSab = settings.EnableSabnzbdDownloader;
        var allowQbit = settings.EnableQBittorrentDownloader;

            // 1) Search Prowlarr for nzb/magnet results
            logger.LogInformation("[Prowlarr] Begin provider for {Artist} - {Release}", artistName, releaseTitle);
            try { relLogger.Info($"[Prowlarr] Begin provider for {artistName} - {releaseTitle}"); } catch { }
            var results = await prowlarr.SearchAlbumAsync(artistName, releaseTitle, cancellationToken);
            logger.LogInformation("[Prowlarr] Search returned {Count} results for {Artist} - {Release}", results.Count, artistName, releaseTitle);
            try { relLogger.Info($"[Prowlarr] Search returned {results.Count} results"); } catch { }
        // Prefer items that look like full releases and larger sizes first
        // Filter and score results for music albums only; reject obvious mismatches (e.g., TV packs)
        results = results
            .Where(r => IsLikelyMusicAlbum(r, artistName, releaseTitle))
            .OrderByDescending(r => Score(r, artistName, releaseTitle))
            .ThenByDescending(r => r.Size ?? 0)
            .ToList();
            if (results.Count == 0) { try { relLogger.Warn("[Prowlarr] No results after filtering"); } catch { } return false; }

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
        static bool LooksLikeDiscography(string? title)
        {
            if (string.IsNullOrWhiteSpace(title)) return false;
            var t = title.ToLowerInvariant();
            string[] bad = ["discography", "collection", "anthology", "complete", "box set", "boxset", "complete works", "singles collection", "mega pack"];
            return bad.Any(k => t.Contains(k));
        }
        var nzb = results.FirstOrDefault(r =>
            !string.IsNullOrWhiteSpace(r.DownloadUrl)
            && !LooksLikeTorrentUrl(r.DownloadUrl!)
            && TitleMatches(r.Title)
            && !LooksLikeDiscography(r.Title)
        );
            if (allowSab && nzb is not null && !string.IsNullOrWhiteSpace(nzb.DownloadUrl))
        {
            // Always fetch NZB bytes and upload them to SABnzbd to avoid SAB needing network access to Prowlarr
            var url = EnsureProwlarrApiKey(nzb.DownloadUrl!);
            logger.LogInformation("[Prowlarr] Uploading NZB to SABnzbd: {Title}", nzb.Title ?? "(no title)");
            try
            {
                try { relLogger.Info($"[Prowlarr] Uploading NZB to SABnzbd: {nzb.Title}"); } catch { }
                using var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync(url, cancellationToken);
                // Validate the payload looks like an NZB, not a torrent
                if (!LooksLikeNzb(bytes))
                {
                    logger.LogWarning("[Prowlarr] Fetched content does not look like NZB, skipping upload (likely torrent)");
                    try { relLogger.Warn("[Prowlarr] Content not NZB; skipping"); } catch { }
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
                if (okUpload) { try { relLogger.Info("[Prowlarr] SABnzbd accepted NZB upload"); } catch { } return true; }
                logger.LogWarning("[Prowlarr] SABnzbd rejected NZB content upload");
                try { relLogger.Warn("[Prowlarr] SABnzbd rejected NZB content upload"); } catch { }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[Prowlarr] Failed fetching/uploading NZB content");
                try { relLogger.Error($"[Prowlarr] Exception during NZB upload: {ex.Message}"); } catch { }
            }
        }

        TryMagnet:
        var magnet = results.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.MagnetUrl) && TitleMatches(r.Title) && !LooksLikeDiscography(r.Title));
        if (allowQbit && magnet is not null && !string.IsNullOrWhiteSpace(magnet.MagnetUrl))
        {
            logger.LogInformation("[Prowlarr] Handing off magnet to qBittorrent: {Title}", magnet.Title ?? "(no title)");
            try { relLogger.Info($"[Prowlarr] Handing off magnet to qBittorrent: {magnet.Title}"); } catch { }
            var ok = await qb.AddMagnetAsync(magnet.MagnetUrl!, null, cancellationToken);
            if (ok) { try { relLogger.Info("[Prowlarr] qBittorrent accepted magnet"); } catch { } return true; }
            logger.LogWarning("[Prowlarr] qBittorrent did not accept magnet handoff");
            try { relLogger.Warn("[Prowlarr] qBittorrent did not accept magnet"); } catch { }
        }

        // Fallbacks by type: if we have a torrent URL, send to qBittorrent; if generic HTTP and NZB-like, send to SAB
        var torrentUrl = results.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.DownloadUrl) && LooksLikeTorrentUrl(r.DownloadUrl!) && TitleMatches(r.Title) && !LooksLikeDiscography(r.Title))?.DownloadUrl;
        if (allowQbit && !string.IsNullOrWhiteSpace(torrentUrl))
        {
            logger.LogInformation("[Prowlarr] Handing off .torrent URL to qBittorrent");
            try { relLogger.Info("[Prowlarr] Handing off .torrent URL to qBittorrent"); } catch { }
            var okT = await qb.AddByUrlAsync(EnsureProwlarrApiKey(torrentUrl!), null, cancellationToken);
            if (okT) { try { relLogger.Info("[Prowlarr] qBittorrent accepted .torrent URL"); } catch { } return true; }
        }

        var httpUrl = results.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.DownloadUrl) && TitleMatches(r.Title) && !LooksLikeDiscography(r.Title))?.DownloadUrl;

        // Discography path (transparent): if discography-like and allowed, handoff to SAB/qBit into staging
        var anyDiscography = results.FirstOrDefault(r => LooksLikeDiscography(r.Title));
        if (anyDiscography != null && settings.DiscographyEnabled)
        {
            var staging = settings.DiscographyStagingPath;
            if (!string.IsNullOrWhiteSpace(staging))
            {
                // Prefer NZB otherwise magnet/torrent URL
                var discNzb = results.FirstOrDefault(r => LooksLikeDiscography(r.Title) && !string.IsNullOrWhiteSpace(r.DownloadUrl) && !LooksLikeTorrentUrl(r.DownloadUrl!));
                if (allowSab && discNzb is not null)
                {
                    try
                    {
                        var url = EnsureProwlarrApiKey(discNzb.DownloadUrl!);
                        using var http = new HttpClient();
                        var bytes = await http.GetByteArrayAsync(url, cancellationToken);
                        if (LooksLikeNzb(bytes))
                        {
                            var ok = await sab.AddNzbByContentAsync(bytes, (discNzb.Title ?? "discography").Replace(' ', '+') + ".nzb", cancellationToken, pathOverride: $"discography/{artistId}", nzbName: $"{artistName} - DISCography bundle");
                            if (ok) { try { relLogger.Info("[Prowlarr] Discography NZB handed to SAB for staging"); } catch { } }
                        }
                    }
                    catch { }
                }
                else if (allowQbit)
                {
                    var discTorrent = results.FirstOrDefault(r => LooksLikeDiscography(r.Title) && !string.IsNullOrWhiteSpace(r.DownloadUrl) && LooksLikeTorrentUrl(r.DownloadUrl!))?.DownloadUrl
                                     ?? results.FirstOrDefault(r => LooksLikeDiscography(r.Title) && !string.IsNullOrWhiteSpace(r.MagnetUrl))?.MagnetUrl;
                    if (!string.IsNullOrWhiteSpace(discTorrent))
                    {
                        try { relLogger.Info("[Prowlarr] Discography torrent/magnet handed to qBittorrent for staging"); } catch { }
                        await qb.AddByUrlAsync(EnsureProwlarrApiKey(discTorrent!), null, cancellationToken);
                    }
                }
            }
        }
        if (allowSab && !string.IsNullOrWhiteSpace(httpUrl) && (httpUrl!.StartsWith("http://") || httpUrl.StartsWith("https://")))
        {
            var url2 = EnsureProwlarrApiKey(httpUrl);
            logger.LogInformation("[Prowlarr] Attempting SABnzbd handoff using HTTP URL: {Url}", url2);
            try { relLogger.Info($"[Prowlarr] Attempting SABnzbd handoff using HTTP URL: {url2}"); } catch { }
            var ok2 = await sab.AddNzbByUrlAsync(url2, $"{artistName} - {releaseTitle}", cancellationToken);
            if (ok2) { try { relLogger.Info("[Prowlarr] SABnzbd accepted HTTP handoff"); } catch { } return true; }
        }

            // Diagnostic: log a few candidates and reasons
            try
            {
                foreach (var r in results.Take(3))
                {
                    var reasons = new List<string>();
                    if (!TitleMatches(r.Title)) reasons.Add("titleMismatch");
                    if (LooksLikeDiscography(r.Title)) reasons.Add("discography");
                    if (string.IsNullOrWhiteSpace(r.DownloadUrl) && string.IsNullOrWhiteSpace(r.MagnetUrl)) reasons.Add("noLink");
                    logger.LogInformation("[Prowlarr] Candidate: title={Title} size={Size} reasons={Reasons}", r.Title ?? "", r.Size, string.Join(',', reasons));
                    try { relLogger.Info($"[Prowlarr] Candidate: title={r.Title} size={r.Size} reasons=[{string.Join(',', reasons)}]"); } catch { }
                }
            }
            catch { }

            logger.LogInformation("Prowlarr provider found results but failed to hand off to downloader (no acceptable candidates)");
            try { relLogger.Warn("[Prowlarr] No acceptable candidates after filtering. First few candidates were logged above with reasons."); } catch { }
            return false;
        }
        finally
        {
            relLoggerImpl?.Dispose();
        }
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


