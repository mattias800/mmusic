using System.Text;
using Microsoft.Extensions.Logging;

namespace MusicGQL.Features.External.Downloads.Prowlarr;

internal static class ProwlarrQueryBuilder
{
    // Audio categories to request from Prowlarr. Repeated categories are required, not comma-separated.
    private static readonly int[] AudioCategories = [3000, 3010, 3040, 3050];

    public static IReadOnlyList<string> BuildCandidateUrls(
        string baseUrl,
        string apiKey,
        string query,
        int[]? indexerIds,
        ILogger? logger = null
    )
    {
        var safeBase = baseUrl.TrimEnd('/');
        var q = Uri.EscapeDataString(query);
        var key = Uri.EscapeDataString(apiKey);

        string CatParams() => string.Concat(AudioCategories.Select(c => $"&categories={c}"));
        string IndexerParams() =>
            indexerIds is { Length: > 0 }
                ? string.Concat(indexerIds.Select(i => $"&indexers={i}"))
                : string.Empty;

        // Simplified, deterministic URL set using only 'query' param with repeated audio categories.
        var urls = new List<string>
        {
            $"{safeBase}/api/v1/search?apikey={key}&query={q}{CatParams()}{IndexerParams()}",
            $"{safeBase}/api/v1/search?apikey={key}&query={q}{CatParams()}{IndexerParams()}&limit=50",
        };

        logger?.LogDebug(
            "[Prowlarr] Built {Count} candidate URLs (indexers: {Idx})",
            urls.Count,
            indexerIds is { Length: > 0 } ? string.Join(',', indexerIds) : "<none>"
        );

        return urls;
    }
}
