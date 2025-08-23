namespace MusicGQL.Features.External.Downloads.Prowlarr;

internal enum ProwlarrSelectionType
{
    None,
    Nzb,
    Magnet,
    Torrent
}

internal sealed record ProwlarrSelection(ProwlarrSelectionType Type, ProwlarrRelease? Release, string? UrlOrMagnet, bool IsDiscography, string Reason = "");

internal static class ProwlarrSelectionLogic
{
    internal static ProwlarrSelection Decide(
        IReadOnlyList<ProwlarrRelease> results,
        string artistName,
        string releaseTitle,
        bool allowSab,
        bool allowQbit,
        bool discographyEnabled)
    {
        // Prefer NZB (non-torrent http url) when SAB is allowed
        if (allowSab)
        {
            var nzb = results.FirstOrDefault(r =>
                !string.IsNullOrWhiteSpace(r.DownloadUrl)
                && !LooksLikeTorrentUrl(r.DownloadUrl!)
                && ProwlarrTextMatch.TitleMatches(r.Title, artistName, releaseTitle)
                && !LooksLikeDiscography(r.Title)
            );
            if (nzb is not null)
            {
                return new ProwlarrSelection(ProwlarrSelectionType.Nzb, nzb, nzb.DownloadUrl, false);
            }
        }

        // Prefer magnet for qBittorrent
        if (allowQbit)
        {
            var magnet = results.FirstOrDefault(r =>
                !string.IsNullOrWhiteSpace(r.MagnetUrl)
                && ProwlarrTextMatch.TitleMatches(r.Title, artistName, releaseTitle)
                && !LooksLikeDiscography(r.Title)
            );
            if (magnet is not null)
            {
                return new ProwlarrSelection(ProwlarrSelectionType.Magnet, magnet, magnet.MagnetUrl, false);
            }
        }

        // Fallback to torrent url for qBittorrent
        if (allowQbit)
        {
            var torrentUrl = results.FirstOrDefault(r =>
                !string.IsNullOrWhiteSpace(r.DownloadUrl) &&
                LooksLikeTorrentUrl(r.DownloadUrl!) &&
                ProwlarrTextMatch.TitleMatches(r.Title, artistName, releaseTitle) &&
                !LooksLikeDiscography(r.Title)
            );
            if (torrentUrl is not null)
            {
                return new ProwlarrSelection(ProwlarrSelectionType.Torrent, torrentUrl, torrentUrl.DownloadUrl, false);
            }
        }

        // Discography handling as last resort if explicitly enabled
        if (discographyEnabled)
        {
            if (allowSab)
            {
                var discNzb = results.FirstOrDefault(r =>
                    LooksLikeDiscography(r.Title) && !string.IsNullOrWhiteSpace(r.DownloadUrl) && !LooksLikeTorrentUrl(r.DownloadUrl!));
                if (discNzb is not null)
                {
                    return new ProwlarrSelection(ProwlarrSelectionType.Nzb, discNzb, discNzb.DownloadUrl, true);
                }
            }

            if (allowQbit)
            {
                var discTorrent = results.FirstOrDefault(r =>
                    LooksLikeDiscography(r.Title) && !string.IsNullOrWhiteSpace(r.DownloadUrl) && LooksLikeTorrentUrl(r.DownloadUrl!));
                if (discTorrent is not null)
                {
                    return new ProwlarrSelection(ProwlarrSelectionType.Torrent, discTorrent, discTorrent.DownloadUrl, true);
                }

                var discMagnet = results.FirstOrDefault(r =>
                    LooksLikeDiscography(r.Title) && !string.IsNullOrWhiteSpace(r.MagnetUrl));
                if (discMagnet is not null)
                {
                    return new ProwlarrSelection(ProwlarrSelectionType.Magnet, discMagnet, discMagnet.MagnetUrl, true);
                }
            }
        }

        return new ProwlarrSelection(ProwlarrSelectionType.None, null, null, false, "No acceptable candidates found");
    }

    internal static bool TitleMatches(string? title, string artistName, string releaseTitle)
    {
        if (string.IsNullOrWhiteSpace(title)) return false;
        var t = title.ToLowerInvariant();
        var a = (artistName ?? string.Empty).ToLowerInvariant();
        var rel = (releaseTitle ?? string.Empty).ToLowerInvariant();
        if (!t.Contains(a)) return false;
        var albumWords = rel.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleWords = t.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var matchCount = albumWords.Count(w => titleWords.Any(x => x.Contains(w) || w.Contains(x)));
        return matchCount >= Math.Max(1, albumWords.Length / 2);
    }

    internal static bool LooksLikeDiscography(string? title)
    {
        if (string.IsNullOrWhiteSpace(title)) return false;
        var t = title.ToLowerInvariant();
        string[] bad = [
            "discography", "collection", "anthology", "complete", "box set", "boxset",
            "complete works", "singles collection", "mega pack"
        ];
        return bad.Any(k => t.Contains(k));
    }

    internal static bool LooksLikeTorrentUrl(string downloadUrl)
    {
        try
        {
            return downloadUrl.EndsWith(".torrent", StringComparison.OrdinalIgnoreCase)
                   || downloadUrl.Contains("torrent", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}

