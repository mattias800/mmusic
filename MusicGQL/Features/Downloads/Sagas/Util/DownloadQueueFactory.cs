using Soulseek;

namespace MusicGQL.Features.Downloads.Sagas.Util;

public static class DownloadQueueFactory
{
    public static Queue<DownloadQueueItem> Create(SearchResponse searchResponse) =>
        new(
            searchResponse
                .Files.Select(
                    (file, i) =>
                    {
                        var safe = SanitizeFileName(System.IO.Path.GetFileNameWithoutExtension(file.Filename));
                        var ext = System.IO.Path.GetExtension(file.Filename);
                        var local = string.IsNullOrWhiteSpace(safe) ? $"track_{i}{ext}" : $"{safe}{ext}";
                        return new DownloadQueueItem(
                            searchResponse.Username,
                            file.Filename,
                            local
                        );
                    }
                )
                .ToList()
        );

    private static string SanitizeFileName(string name)
    {
        var invalid = System.IO.Path.GetInvalidFileNameChars();
        var sanitized = string.Join("", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Trim();
    }
}
