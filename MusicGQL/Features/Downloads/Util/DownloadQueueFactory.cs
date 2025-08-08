using Soulseek;

namespace MusicGQL.Features.Downloads.Util;

public record DownloadQueueItem(string Username, string FileName, string LocalFileName);

public static class DownloadQueueFactory
{
    public static Queue<DownloadQueueItem> Create(SearchResponse searchResponse)
    {
        var ordered = searchResponse.Files
            .Select(file =>
            {
                var normalized = file.Filename.Replace('\\', '/');
                var justFile = normalized.Split('/').Last();
                var nameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(justFile);
                var ext = System.IO.Path.GetExtension(justFile);
                var safe = SanitizeFileName(nameWithoutExt);
                var lead = ExtractLeadingTrackNumber(safe);
                return new { file, safe, ext, lead, justFile };
            })
            .OrderBy(x => x.lead ?? int.MaxValue)
            .ThenBy(x => x.justFile, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var queueItems = ordered
            .Select((x, i) =>
            {
                var indexPrefix = (i + 1).ToString("D2");
                var baseName = string.IsNullOrWhiteSpace(x.safe) ? $"track_{i + 1}" : x.safe;
                var local = $"{indexPrefix} - {baseName}{x.ext}";
                return new DownloadQueueItem(searchResponse.Username, x.file.Filename, local);
            })
            .ToList();

        return new Queue<DownloadQueueItem>(queueItems);
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
}
