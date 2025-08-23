using System.Text.Json;

namespace MusicGQL.Features.External.Downloads.Prowlarr;

internal static class ProwlarrJsonParser
{
    public static List<ProwlarrRelease> ParseResults(
        JsonElement root,
        string artistName,
        string releaseTitle,
        ILogger logger)
    {
        // Root can be array or object with Results/results
        JsonElement array = root;
        if (root.ValueKind == JsonValueKind.Object)
        {
            if (!TryGetPropertyCI(root, "results", out array) && !TryGetPropertyCI(root, "Results", out array))
            {
                if (!TryGetPropertyCI(root, "data", out array) && !TryGetPropertyCI(root, "Data", out array))
                {
                    array = root;
                }
            }
        }

        if (array.ValueKind != JsonValueKind.Array)
        {
            return new List<ProwlarrRelease>();
        }

        var list = new List<ProwlarrRelease>();
        var totalResults = 0;
        var filteredResults = 0;

        foreach (var item in array.EnumerateArray())
        {
            totalResults++;
            string? title = GetStringCI(item, "title") ?? GetStringCI(item, "Title");
            string? guid = GetStringCI(item, "guid") ?? GetStringCI(item, "Guid");
            string? magnet = GetStringCI(item, "magnetUrl") ?? GetStringCI(item, "MagnetUrl");
            string? downloadUrl = GetStringCI(item, "downloadUrl") ?? GetStringCI(item, "DownloadUrl")
                ?? GetStringCI(item, "link") ?? GetStringCI(item, "Link");
            long? size = GetInt64CI(item, "size") ?? GetInt64CI(item, "Size");
            int? indexerId = null;
            if (TryGetPropertyCI(item, "indexerId", out var idx) && idx.ValueKind == JsonValueKind.Number)
            {
                if (idx.TryGetInt32(out var i32)) indexerId = i32;
            }

            var release = new ProwlarrRelease(title, guid, magnet, downloadUrl, size, indexerId);

            if (ProwlarrResultFilter.IsValidMusicResult(release, artistName, releaseTitle))
            {
                list.Add(release);
            }
            else
            {
                filteredResults++;
                var reason = ProwlarrResultFilter.GetRejectionReason(release, artistName, releaseTitle);
                logger.LogDebug("[Prowlarr] Filtered out result '{Title}': {Reason}", title, reason);
            }
        }

        logger.LogInformation("[Prowlarr] Filtered {Filtered}/{Total} results for '{Artist} - {Album}'",
            filteredResults, totalResults, artistName, releaseTitle);

        list.Sort((a, b) => ProwlarrScorer.CalculateRelevanceScore(b, artistName, releaseTitle)
            .CompareTo(ProwlarrScorer.CalculateRelevanceScore(a, artistName, releaseTitle)));

        return list;
    }

    public static void LogRootShape(JsonElement root, ILogger logger)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            logger.LogDebug("[Prowlarr] Root is array; first item keys: {Keys}",
                root.GetArrayLength() > 0
                    ? string.Join(",", root[0].EnumerateObject().Select(p => p.Name))
                    : "(empty)");
            return;
        }

        if (root.ValueKind == JsonValueKind.Object)
        {
            var keys = root.EnumerateObject().Select(p => p.Name).ToList();
            logger.LogDebug("[Prowlarr] Root is object; keys: {Keys}", string.Join(",", keys));
            foreach (var k in keys)
            {
                if (TryGetPropertyCI(root, k, out var v) && v.ValueKind == JsonValueKind.Array &&
                    v.GetArrayLength() > 0)
                {
                    var innerKeys = v[0].EnumerateObject().Select(p => p.Name);
                    logger.LogDebug("[Prowlarr] First element keys of '{Key}': {InnerKeys}", k,
                        string.Join(",", innerKeys));
                }
            }
        }
        else
        {
            logger.LogDebug("[Prowlarr] Root is {Kind}", root.ValueKind);
        }
    }

    public static bool TryGetPropertyCI(JsonElement obj, string name, out JsonElement value)
    {
        if (obj.ValueKind != JsonValueKind.Object)
        {
            value = default;
            return false;
        }

        foreach (var prop in obj.EnumerateObject())
        {
            if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = prop.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    public static string? GetStringCI(JsonElement obj, string name)
    {
        if (TryGetPropertyCI(obj, name, out var v) && v.ValueKind == JsonValueKind.String)
        {
            return v.GetString();
        }

        return null;
    }

    public static long? GetInt64CI(JsonElement obj, string name)
    {
        if (TryGetPropertyCI(obj, name, out var v) && v.ValueKind == JsonValueKind.Number)
        {
            if (v.TryGetInt64(out var i64)) return i64;
        }

        return null;
    }
}

