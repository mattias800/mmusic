using System.Text.RegularExpressions;

namespace MusicGQL.Features.Downloads.Util;

public static class FileNameParsing
{
    // Extract disc and track numbers from a file or folder name.
    // - Disc: matches "cd|disc|disk|digital media" + number; defaults to 1 if none
    // - Track: prefers embedded "- NN -"; falls back to first leading number; normalizes 3+ digit encodings
    public static (int disc, int track) ExtractDiscTrackFromName(string? name)
    {
        int disc = 1;
        int track = -1;
        try
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var lower = name!.ToLowerInvariant();
                var discMatch = Regex.Match(
                    lower,
                    "\\b(?:cd|disc|disk|digital\\s*media)\\s*(\\d+)\\b",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled
                );
                if (discMatch.Success && int.TryParse(discMatch.Groups[1].Value, out var d)) disc = d;

                // Determine start position for track search: after the disc label if present
                int searchStart = discMatch.Success ? discMatch.Index + discMatch.Length : 0;

                // Prefer embedded "- NN -" pattern from the searchStart position
                var embedded = Regex.Match(
                    name.Substring(Math.Min(searchStart, name.Length)),
                    "-\\s*(\\d{1,3})\\s*-",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled
                );
                if (embedded.Success && int.TryParse(embedded.Groups[1].Value, out var t2))
                {
                    track = t2;
                }

                if (track < 0)
                {
                    // Fallback: first standalone number after searchStart
                    var numsAfter = Regex.Match(
                        name.Substring(Math.Min(searchStart, name.Length)),
                        "\\b(\\d{1,4})\\b",
                        RegexOptions.IgnoreCase | RegexOptions.Compiled
                    );
                    if (numsAfter.Success && int.TryParse(numsAfter.Groups[1].Value, out var n1))
                    {
                        track = n1 > 99 ? (n1 % 100 == 0 ? n1 : n1 % 100) : n1;
                    }
                }

                if (track < 0)
                {
                    // Last resort: scan from the start (legacy behavior)
                    var span = name.AsSpan();
                    int pos = 0;
                    while (pos < span.Length && !char.IsDigit(span[pos])) pos++;
                    int start = pos;
                    while (pos < span.Length && char.IsDigit(span[pos])) pos++;
                    if (pos > start && int.TryParse(span.Slice(start, pos - start), out var n))
                    {
                        track = n > 99 ? (n % 100 == 0 ? n : n % 100) : n;
                    }
                }
            }
        }
        catch { }
        return (disc, track);
    }
}
