using Microsoft.Extensions.Logging;

namespace MusicGQL.Features.External.Downloads.Prowlarr;

internal static class ProwlarrSearchStrategy
{
    /// <summary>
    /// Build the ordered list of search queries to try against Prowlarr.
    /// Order: base (artist + album) → base + year (when provided) → base + " 320" → base + " FLAC".
    /// </summary>
    public static IReadOnlyList<string> BuildQueries(string artistName, string releaseTitle, int? year, ILogger logger)
    {
        var baseBroad = ($"{artistName} {releaseTitle}").Trim();
        var queries = new List<string> { baseBroad };

        if (year.HasValue)
        {
            queries.Add($"{baseBroad} {year.Value}");
        }

        queries.Add($"{baseBroad} 320");
        queries.Add($"{baseBroad} FLAC");

        logger.LogDebug("[Prowlarr] Built {Count} search queries (broad-first): {Queries}", queries.Count, string.Join(" | ", queries));
        return queries;
    }
}

