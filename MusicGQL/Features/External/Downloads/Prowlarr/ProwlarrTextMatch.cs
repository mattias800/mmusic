using System.Globalization;
using System.Text;

namespace MusicGQL.Features.External.Downloads.Prowlarr;

internal static class ProwlarrTextMatch
{
    public static bool TitleMatches(string? title, string artistName, string releaseTitle)
    {
        if (string.IsNullOrWhiteSpace(title))
            return false;
        var t = Normalize(title);
        var album = Normalize(releaseTitle);
        var artistWords = Normalize(artistName).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleWords = t.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        // Require at least half of artist words to fuzzy-match words in title
        var artistMatchCount = artistWords.Count(word =>
            titleWords.Any(x => FuzzyWordMatch(x, word))
        );
        if (artistMatchCount < Math.Max(1, artistWords.Length / 2))
            return false;
        // Require at least half of album words, with a stricter rule for single-word album titles
        var albumWords = album.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (albumWords.Length == 1)
        {
            // For single-word album titles (e.g., "Color"), allow fuzzy but enforce reasonable length proximity
            // and explicitly reject simple pluralization/singularization
            var token = albumWords[0];
            return titleWords.Any(x => SingleWordAlbumTokenMatch(x, token));
        }
        else
        {
            var albumMatchCount = albumWords.Count(word =>
                titleWords.Any(x => FuzzyWordMatch(x, word))
            );
            return albumMatchCount >= Math.Max(1, albumWords.Length / 2);
        }
    }

    public static bool ContainsAlbumTitleFuzzy(string title, string albumTitle)
    {
        var t = Normalize(title);
        var album = Normalize(albumTitle);
        var albumWords = album.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleWords = t.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (albumWords.Length == 1)
        {
            var token = albumWords[0];
            return titleWords.Any(x => SingleWordAlbumTokenMatch(x, token));
        }
        var matchCount = albumWords.Count(word => titleWords.Any(x => FuzzyWordMatch(x, word)));
        return matchCount >= Math.Max(1, albumWords.Length / 2);
    }

    public static bool FuzzyWordMatch(string a, string b)
    {
        if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b))
            return false;
        if (a == b)
            return true;

        int minLen = Math.Min(a.Length, b.Length);
        int maxLen = Math.Max(a.Length, b.Length);

        // For very short tokens (<3 chars), do not allow fuzzy matches (only exact match above)
        if (minLen < 3)
            return false;

        // Do not treat plain substring presence as a fuzzy match to avoid false positives like
        // 'differentartist' vs 'artist'. Rely on prefix similarity and edit distance instead.
        if (minLen >= 4)
        {
            // Common prefix heuristic for long tokens
            int cpl = CommonPrefixLength(a, b);
            if (cpl >= 6 && minLen >= 8)
                return true;

            // Levenshtein with length-aware threshold
            int dist = Levenshtein(a, b);
            int threshold = maxLen <= 5 ? 1 : (maxLen <= 8 ? 2 : 3);
            if (dist <= threshold)
                return true;

            // Relative similarity
            double sim = 1.0 - (double)dist / maxLen;
            if (sim >= 0.8)
                return true;
        }

        return false;
    }

    private static int CommonPrefixLength(string a, string b)
    {
        int n = Math.Min(a.Length, b.Length);
        int i = 0;
        while (i < n && a[i] == b[i])
            i++;
        return i;
    }

    private static bool IsSimplePluralization(string a, string b)
    {
        // Assumes inputs are already normalized/lowercased tokens
        if (a == b) return false;

        // Helper to check if y is a simple plural of x
        static bool IsPluralOf(string x, string y)
        {
            if (string.IsNullOrEmpty(x) || string.IsNullOrEmpty(y)) return false;
            if (y == x + "s") return true;
            if (y == x + "es") return true;
            if (x.EndsWith("y") && y == x[..^1] + "ies") return true;
            return false;
        }

        return IsPluralOf(a, b) || IsPluralOf(b, a);
    }

    private static bool SingleWordAlbumTokenMatch(string titleToken, string albumToken)
    {
        if (string.IsNullOrWhiteSpace(titleToken) || string.IsNullOrWhiteSpace(albumToken))
            return false;
        if (titleToken == albumToken)
            return true;
        if (IsSimplePluralization(titleToken, albumToken))
            return false;
        int lenDiff = Math.Abs(titleToken.Length - albumToken.Length);
        // Disallow tokens that are far in length (e.g., star vs starboy, color vs colorful)
        if (lenDiff > 2)
            return false;
        // For small length differences allow fuzzy matching
        if (lenDiff == 2)
        {
            int dist = Levenshtein(titleToken, albumToken);
            double sim = 1.0 - (double)dist / Math.Max(titleToken.Length, albumToken.Length);
            return dist <= 2 && sim >= 0.8;
        }
        // lenDiff 0 or 1: use the general fuzzy logic
        return FuzzyWordMatch(titleToken, albumToken);
    }

    private static int Levenshtein(string a, string b)
    {
        int n = a.Length,
            m = b.Length;
        if (n == 0)
            return m;
        if (m == 0)
            return n;
        var d = new int[n + 1, m + 1];
        for (int i = 0; i <= n; i++)
            d[i, 0] = i;
        for (int j = 0; j <= m; j++)
            d[0, j] = j;
        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost
                );
            }
        }
        return d[n, m];
    }

    private static string Normalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        s = s.ToLowerInvariant();

        // Unicode normalize and strip diacritics (é -> e, å -> a, ö -> o, etc.)
        var decomposed = s.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder(decomposed.Length * 2);
        foreach (var ch in decomposed)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (cat == UnicodeCategory.NonSpacingMark)
                continue; // drop diacritic marks

            // Map common special letters to ASCII-ish approximations
            switch (ch)
            {
                case 'ß': sb.Append("ss"); continue;
                case 'Æ': case 'æ': sb.Append("ae"); continue;
                case 'Œ': case 'œ': sb.Append("oe"); continue;
                case 'Ø': case 'ø': sb.Append("o"); continue;
                case 'Ð': case 'ð': sb.Append("d"); continue;
                case 'Þ': case 'þ': sb.Append("th"); continue;
            }

            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
            }
            else if (char.IsWhiteSpace(ch) || char.IsPunctuation(ch) || char.IsSymbol(ch))
            {
                // Treat punctuation/symbols as separators to avoid token concatenation (e.g., ac/dc -> ac dc)
                sb.Append(' ');
            }
            // else: ignore other categories
        }

        var collapsed = System.Text.RegularExpressions.Regex.Replace(sb.ToString(), "\\s+", " ").Trim();
        return collapsed;
    }
}
