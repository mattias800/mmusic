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
        // Require at least half of album words
        var albumWords = album.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var albumMatchCount = albumWords.Count(word =>
            titleWords.Any(x => FuzzyWordMatch(x, word))
        );
        return albumMatchCount >= Math.Max(1, albumWords.Length / 2);
    }

    public static bool ContainsAlbumTitleFuzzy(string title, string albumTitle)
    {
        var t = Normalize(title);
        var album = Normalize(albumTitle);
        var albumWords = album.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleWords = t.Split(' ', StringSplitOptions.RemoveEmptyEntries);
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

        // Substring match only counts when both tokens are reasonably long
        if (minLen >= 4 && (a.Contains(b) || b.Contains(a)))
            return true;

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
        s = s.ToLowerInvariant();
        var keep = new System.Text.StringBuilder(s.Length);
        foreach (var ch in s)
        {
            if (char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch))
                keep.Append(ch);
        }
        return System.Text.RegularExpressions.Regex.Replace(keep.ToString(), "\\s+", " ").Trim();
    }
}
