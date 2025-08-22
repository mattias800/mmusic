using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Soulseek;

namespace MusicGQL.Features.Downloads.Util;

public record DownloadQueueItem(string Username, string FileName, string LocalFileName, int? SourceTrackNumber = null);

public static class DownloadQueueFactory
{
    public static Queue<DownloadQueueItem> Create(
        SearchResponse searchResponse,
        string artistName,
        string releaseTitle,
        int? expectedTrackCount = null,
        IEnumerable<string>? expectedTrackTitles = null,
        MusicGQL.Features.Downloads.Services.IDownloadLogger? logger = null
    )
    {
        logger ??= new MusicGQL.Features.Downloads.Services.NullDownloadLogger();
        logger.Info($"[QueueFactory] Begin create for {artistName} - {releaseTitle} (expectedCount={(expectedTrackCount?.ToString() ?? "null")}) from user={searchResponse.Username}");
        var nArtist = NormalizeForCompare(artistName);
        var nTitle = NormalizeForCompare(releaseTitle);

var annotated = searchResponse.Files
            .Where(file => file.Extension.Equals("mp3", StringComparison.OrdinalIgnoreCase) && file.BitRate == 320)
            .Select(file => AnnotateFile(file.Filename))
            .ToList();
        try { logger.Info($"[QueueFactory] Annotated candidate files={annotated.Count}"); } catch { }
        if (annotated.Count == 0)
        {
            try { logger.Warn("[QueueFactory] No MP3 320 files in response"); } catch { }
            return new Queue<DownloadQueueItem>();
        }

        var groups = new Dictionary<(string albumKey, int? disc), List<AnnotatedFile>>();
        foreach (var a in annotated)
        {
            var normSegs = a.NormalizedSegments;
            int idxArtist = normSegs.FindIndex(s => s.Equals(nArtist, StringComparison.OrdinalIgnoreCase));
if (idxArtist < 0)
            {
                try { logger.Info($"[QueueFactory] Reject path (no artist segment match): {a.NormalizedPath}"); } catch { }
                continue;
            }

            int idxAlbum = -1;
            for (int i = idxArtist + 1; i < normSegs.Count; i++)
            {
                if (normSegs[i].Contains(nTitle, StringComparison.OrdinalIgnoreCase) ||
                    nTitle.Contains(normSegs[i], StringComparison.OrdinalIgnoreCase))
                {
                    idxAlbum = i;
                    break;
                }
            }
if (idxAlbum < 0)
            {
                try { logger.Info($"[QueueFactory] Reject path (no title segment match after artist): {a.NormalizedPath}"); } catch { }
                continue;
            }

            var disc = TryExtractDiscNumber(a.Segments, startIndex: idxAlbum + 1);
            var albumKeyNorm = NormalizeForCompare(a.Segments[idxAlbum]);
            var key = (albumKeyNorm, disc);
            if (!groups.TryGetValue(key, out var list))
            {
                list = new List<AnnotatedFile>();
                groups[key] = list;
            }
            list.Add(a);
        }

        // Prefer single-disc (disc null or 1), then digital-friendly folder names implicitly via count
var primary = groups
            .OrderByDescending(g => !g.Key.disc.HasValue || g.Key.disc.Value == 1)
            .ThenByDescending(g => g.Value.Count)
            .ThenBy(g => g.Key.disc ?? 0)
            .FirstOrDefault();

        try
        {
            foreach (var kv in groups.OrderByDescending(g => g.Value.Count).Take(5))
            {
                logger.Info($"[QueueFactory] Group albumKey='{kv.Key.albumKey}' disc={(kv.Key.disc?.ToString() ?? "null")} count={kv.Value.Count}");
            }
        }
        catch { }

        List<AnnotatedFile> inPrimary = primary.Value ?? new List<AnnotatedFile>();
        if (inPrimary.Count == 0)
        {
            inPrimary = annotated;
        }

        var expectedTitlesList = (expectedTrackTitles ?? Enumerable.Empty<string>())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();

        bool titleIsExpected(AnnotatedFile af)
        {
            if (expectedTitlesList.Count == 0) return false;
            var baseNoQualifiers = StripQualifiers(af.BaseNameWithoutExt);
            foreach (var expected in expectedTitlesList)
            {
                if (AreTitlesEquivalent(baseNoQualifiers, expected))
                {
                    return true;
                }
            }
            return false;
        }

        string[] exclusionHints =
        [
            "bonus", "live", "re-recorded", "rerecorded", "re recorded", "demo", "karaoke", "instrumental", "remix"
        ];

var filtered = (expectedTitlesList.Count > 0)
            ? inPrimary.Where(a => titleIsExpected(a)).ToList()
            : inPrimary.Where(a => !PathContainsHints(a, exclusionHints)).ToList();
        try
        {
            logger.Info($"[QueueFactory] Filtered inPrimary: before={inPrimary.Count} after={filtered.Count} mode={(expectedTitlesList.Count>0?"expected-titles":"hint-exclusion")}");
            if (filtered.Count == 0 && inPrimary.Count > 0)
            {
                foreach (var a in inPrimary.Take(10))
                {
                    var baseNoQualifiers = StripQualifiers(a.BaseNameWithoutExt);
                    bool exp = expectedTitlesList.Count>0 && expectedTitlesList.Any(t => AreTitlesEquivalent(baseNoQualifiers, t));
                    bool hinted = PathContainsHints(a, exclusionHints);
                    logger.Info($"[QueueFactory] Rejected: file='{a.JustFile}' expectedMatch={exp} hinted={hinted}");
                }
            }
        }
        catch { }

        // If strict expected-title matching yields too few tracks, relax to hint-based filter
        int fallbackMin = expectedTrackCount.HasValue && expectedTrackCount.Value > 0
            ? Math.Max(2, Math.Min(expectedTrackCount.Value, expectedTrackCount.Value - 1))
            : 5;
        if (filtered.Count < fallbackMin)
        {
            var relaxed = inPrimary.Where(a => !PathContainsHints(a, exclusionHints)).ToList();
            if (relaxed.Count >= filtered.Count)
            {
                filtered = relaxed;
            }
        }

        // If still nothing, fall back to all annotated files for this candidate (last resort)
if (filtered.Count == 0)
        {
            try { logger.Warn("[QueueFactory] Filter produced zero items; falling back to inPrimary/annotated"); } catch { }
            filtered = inPrimary.Count > 0 ? inPrimary : annotated;
        }

var orderedAnnotated = filtered
            .Select(a =>
            {
                var rawLead = ExtractLeadingTrackNumber(a.BaseNameWithoutExt);
                var normalizedLead = NormalizeLeadingTrackNumber(rawLead, expectedTrackCount);
                return new { a, lead = normalizedLead, rawLead };
            })
            .OrderBy(x => x.lead ?? int.MaxValue)
            .ThenBy(x => x.a.JustFile, StringComparer.OrdinalIgnoreCase)
            .ToList();
        try
        {
            foreach (var x in orderedAnnotated.Take(20))
            {
                logger.Info($"[QueueFactory] Order: file='{x.a.JustFile}' rawLead={(x.rawLead?.ToString() ?? "null")} lead={(x.lead?.ToString() ?? "null")}");
            }
        }
        catch { }

        var ordered = orderedAnnotated.Select(x => x.a).ToList();

        if (expectedTrackCount.HasValue && expectedTrackCount.Value > 0 && ordered.Count > expectedTrackCount.Value)
        {
            ordered = ordered.Take(expectedTrackCount.Value).ToList();
        }

        var queueItems = orderedAnnotated
            .Select((x, i) =>
            {
                var trackNo = x.lead ?? (i + 1);
                var indexPrefix = trackNo.ToString("D2");
                var baseName = string.IsNullOrWhiteSpace(x.a.SafeBaseName) ? $"track_{trackNo}" : x.a.SafeBaseName;
                var local = $"{indexPrefix} - {baseName}{x.a.Extension}";
                return new DownloadQueueItem(searchResponse.Username, x.a.OriginalFileName, local, x.lead);
            })
            .ToList();

        return new Queue<DownloadQueueItem>(queueItems);
    }

    public static Queue<DownloadQueueItem> Create(SearchResponse searchResponse)
    {
        var annotated = searchResponse.Files
            .Where(file => file.Extension.Equals("mp3", StringComparison.OrdinalIgnoreCase) && file.BitRate == 320)
            .Select(file => AnnotateFile(file.Filename))
            .ToList();

        var orderedAnnotated = annotated
            .Select(a =>
            {
                var rawLead = ExtractLeadingTrackNumber(a.BaseNameWithoutExt);
                var normalizedLead = NormalizeLeadingTrackNumber(rawLead, expectedTrackCount: null);
                return new { a, lead = normalizedLead, rawLead };
            })
            .OrderBy(x => x.lead ?? int.MaxValue)
            .ThenBy(x => x.a.JustFile, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var queueItems = orderedAnnotated
            .Select((x, i) =>
            {
                var trackNo = x.lead ?? (i + 1);
                var indexPrefix = trackNo.ToString("D2");
                var baseName = string.IsNullOrWhiteSpace(x.a.SafeBaseName) ? $"track_{trackNo}" : x.a.SafeBaseName;
                var local = $"{indexPrefix} - {baseName}{x.a.Extension}";
                return new DownloadQueueItem(searchResponse.Username, x.a.OriginalFileName, local, x.lead);
            })
            .ToList();

        return new Queue<DownloadQueueItem>(queueItems);
    }

    private sealed class AnnotatedFile
    {
        public required string OriginalFileName { get; init; }
        public required string NormalizedPath { get; init; }
        public required string JustFile { get; init; }
        public required string BaseNameWithoutExt { get; init; }
        public required string Extension { get; init; }
        public required string SafeBaseName { get; init; }
        public required List<string> Segments { get; init; }
        public required List<string> NormalizedSegments { get; init; }
    }

    private static AnnotatedFile AnnotateFile(string fileName)
    {
        var normalized = fileName.Replace('\\', '/');
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
        var normSegs = segments.Select(NormalizeForCompare).ToList();
        var justFile = segments.LastOrDefault() ?? fileName;
        var nameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(justFile);
        var ext = System.IO.Path.GetExtension(justFile);
        var trimmedBase = RemoveLeadingTrackNumberPrefix(nameWithoutExt);
        var safe = SanitizeFileName(trimmedBase);
        return new AnnotatedFile
        {
            OriginalFileName = fileName,
            NormalizedPath = normalized,
            JustFile = justFile,
            BaseNameWithoutExt = nameWithoutExt,
            Extension = ext,
            SafeBaseName = safe,
            Segments = segments,
            NormalizedSegments = normSegs,
        };
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = System.IO.Path.GetInvalidFileNameChars();
        var sanitized = string.Join("", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Trim();
    }

    private static int? ExtractLeadingTrackNumber(string name)
    {
        // Patterns like "01 - ...", "1.", "1 ", "01_"
        var span = name.AsSpan();
        int pos = 0;
        while (pos < span.Length && char.IsWhiteSpace(span[pos])) pos++;
        int start = pos;
        while (pos < span.Length && char.IsDigit(span[pos])) pos++;
        if (pos > start)
        {
            if (int.TryParse(span.Slice(start, pos - start), out var n))
            {
                return n;
            }
        }
        return null;
    }

    private static string RemoveLeadingTrackNumberPrefix(string name)
    {
        // Remove patterns like "01 - ", "1.", "01_", "01 " from the start
        var span = name.AsSpan();
        int pos = 0;
        while (pos < span.Length && char.IsWhiteSpace(span[pos])) pos++;
        int start = pos;
        while (pos < span.Length && char.IsDigit(span[pos])) pos++;
        if (pos > start)
        {
            // Skip common separators following the number
            while (pos < span.Length && (span[pos] == '-' || span[pos] == '.' || span[pos] == '_' || span[pos] == ' '))
            {
                pos++;
                // If it's a dash followed by a space, skip the space too
                if (pos < span.Length && span[pos - 1] == '-' && span[pos] == ' ') pos++;
            }
            return span.Slice(pos).ToString();
        }
        return name;
    }

    private static int? NormalizeLeadingTrackNumber(int? rawLead, int? expectedTrackCount)
    {
        if (!rawLead.HasValue) return null;
        var n = rawLead.Value;
        if (n <= 99) return n;
        // Handle common disc+track encodings like 103, 208, 1103 etc → use last two digits when plausible
        int lastTwo = n % 100;
        if (lastTwo == 0)
        {
            // Avoid mapping to 0; fall back to raw
            return n;
        }
        if (expectedTrackCount.HasValue && expectedTrackCount.Value > 0)
        {
            if (lastTwo >= 1 && lastTwo <= Math.Min(99, expectedTrackCount.Value))
            {
                return lastTwo;
            }
            return n;
        }
        // Without expected count, assume typical album sizes; bound to 1..30 as a sane default
        if (lastTwo >= 1 && lastTwo <= 30)
        {
            return lastTwo;
        }
        return n;
    }

    private static int? TryExtractDiscNumber(List<string> segments, int startIndex)
    {
        for (int i = startIndex; i < segments.Count; i++)
        {
            var s = segments[i];
            // Common patterns: "CD - 1", "CD1", "Disc 1", "Disk 1"
            var m = Regex.Match(s, @"\b(?:cd|disc|disk)[\s_-]*([0-9]+)\b", RegexOptions.IgnoreCase);
            if (m.Success && int.TryParse(m.Groups[1].Value, out int d))
            {
                return d;
            }
        }
        return null;
    }

    private static bool PathContainsHints(AnnotatedFile af, string[] hints)
    {
        var lower = af.NormalizedPath.ToLowerInvariant();
        return hints.Any(h => lower.Contains(h));
    }

    private static string NormalizeForCompare(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var lowered = input.ToLowerInvariant();
        var folded = RemoveDiacritics(lowered);
        var filtered = Regex.Replace(folded, @"[^\p{L}\p{N}\s]", " ");
        return Regex.Replace(filtered, @"\s+", " ").Trim();
    }

    private static string RemoveDiacritics(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var normalized = input.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(input.Length);
        for (int i = 0; i < normalized.Length; i++)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(normalized[i]);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(normalized[i]);
            }
        }
        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string StripQualifiers(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        string result = Regex.Replace(input, @"\s*[\(\[][\s\S]*?[\)\]]", " ");
        result = Regex.Replace(result, @"\b(?:live|demo|instrumental|karaoke|remix|re[-\s]?recorded|bonus)\b", " ", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\s+", " ").Trim();
        return result;
    }

    private static bool AreTitlesEquivalent(string a, string b)
    {
        // Normalize both titles (remove qualifiers/punctuation, collapse whitespace)
        string na = NormalizeForCompare(StripQualifiers(RemoveLeadingTrackNumberPrefix(a)));
        string nb = NormalizeForCompare(StripQualifiers(b));
        if (string.IsNullOrWhiteSpace(na) || string.IsNullOrWhiteSpace(nb)) return false;
        if (na.Equals(nb, StringComparison.OrdinalIgnoreCase)) return true;
        // Loose containment match for minor variations
        if (na.Length >= 5 && nb.Contains(na, StringComparison.OrdinalIgnoreCase)) return true;
        if (nb.Length >= 5 && na.Contains(nb, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }
}
