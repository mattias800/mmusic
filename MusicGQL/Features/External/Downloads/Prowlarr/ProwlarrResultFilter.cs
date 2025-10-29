using System.Globalization;
using System.Text;

namespace MusicGQL.Features.External.Downloads.Prowlarr;

internal static class ProwlarrResultFilter
{
    public static bool IsValidMusicResult(
        ProwlarrRelease release,
        string artistName,
        string releaseTitle
    )
    {
        if (string.IsNullOrWhiteSpace(release.Title))
            return false;

        var title = release.Title.ToLowerInvariant();

        if (ContainsNonMusicTerms(title))
            return false;

        // Use the unified fuzzy title matcher which handles separators and single-word rules
        if (!ProwlarrTextMatch.TitleMatches(release.Title, artistName, releaseTitle))
            return false;

        return true;
    }

    public static string GetRejectionReason(
        ProwlarrRelease release,
        string artistName,
        string releaseTitle
    )
    {
        if (string.IsNullOrWhiteSpace(release.Title))
            return "Missing title";

        var title = release.Title.ToLowerInvariant();
        var artist = artistName.ToLowerInvariant();
        var album = releaseTitle.ToLowerInvariant();

        if (ContainsNonMusicTerms(title))
            return "Contains non-music terms (TV/movie/video)";

        if (!ContainsArtistName(title, artist))
            return "Artist name mismatch";

        if (!ContainsAlbumTitle(title, album))
            return "Album title mismatch";

        if (ProwlarrScorer.IsTorrentFile(release.DownloadUrl))
            return "Torrent file (not compatible with SABnzbd)";

        return "Unknown validation failure";
    }

    private static bool ContainsNonMusicTerms(string title)
    {
        var nonMusicTerms = new[]
        {
            "1080p",
            "720p",
            "4k",
            "hdtv",
            "web-dl",
            "bluray",
            "dvdrip",
            "h264",
            "h265",
            "x264",
            "x265",
            "season",
            "episode",
            "s01",
            "s02",
            "e01",
            "e02",
            "complete",
            "series",
            "movie",
            "film",
            "documentary",
            "show",
            "tv",
            "television",
            "subtitle",
            "dub",
            "dubbed",
            "multi",
            "dual",
            "audio",
        };

        return nonMusicTerms.Any(term => title.Contains(term));
    }

    private static bool ContainsArtistName(string title, string artistName)
    {
        // Normalize separators similar to ProwlarrTextMatch to support scene folder names
        static string Norm(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            s = s.ToLowerInvariant();

            var decomposed = s.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder(decomposed.Length * 2);
            foreach (var ch in decomposed)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (cat == UnicodeCategory.NonSpacingMark)
                    continue;
                switch (ch)
                {
                    case 'ß': sb.Append("ss"); continue;
                    case 'Æ': case 'æ': sb.Append("ae"); continue;
                    case 'Œ': case 'œ': sb.Append("oe"); continue;
                    case 'Ø': case 'ø': sb.Append("o"); continue;
                    case 'Ð': case 'ð': sb.Append("d"); continue;
                    case 'Þ': case 'þ': sb.Append("th"); continue;
                }
                if (char.IsLetterOrDigit(ch)) sb.Append(ch);
                else if (char.IsWhiteSpace(ch) || char.IsPunctuation(ch) || char.IsSymbol(ch)) sb.Append(' ');
            }
            return System.Text.RegularExpressions.Regex.Replace(sb.ToString(), "\\s+", " ").Trim();
        }

        var artistWords = Norm(artistName).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleWords = Norm(title).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var matchingWords = artistWords.Count(word =>
            titleWords.Any(titleWord => ProwlarrTextMatch.FuzzyWordMatch(titleWord, word))
        );
        return matchingWords >= Math.Max(1, artistWords.Length / 2);
    }

    private static bool ContainsAlbumTitle(string title, string albumTitle)
    {
        return ProwlarrTextMatch.ContainsAlbumTitleFuzzy(title, albumTitle);
    }
}
