using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using IOPath = System.IO.Path;

namespace MusicGQL.Features.Assets;

public class ExternalAssetStorage(IWebHostEnvironment env, IHttpClientFactory httpClientFactory)
{
    private static readonly Regex FileExtensionRegex = new(
        "\\.(jpg|jpeg|png|webp|gif)(?:\\?|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    private string BaseDirectory => IOPath.Combine(env.ContentRootPath, "ExternalAssets");

    public async Task<string?> SaveCoverImageForPlaylistTrackAsync(
        string playlistId,
        string trackId,
        string imageUrl,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            Directory.CreateDirectory(BaseDirectory);
            var coverDir = IOPath.Combine(BaseDirectory, "coverart", playlistId.ToString());
            Directory.CreateDirectory(coverDir);

            var http = httpClientFactory.CreateClient();
            using var response = await http.GetAsync(imageUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;
            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            // Determine extension
            var ext =
                GetExtensionFromContentType(response.Content.Headers.ContentType)
                ?? GetExtensionFromUrl(imageUrl)
                ?? ".jpg";

            var fileName = SanitizeFileName(trackId) + ext;
            var filePath = IOPath.Combine(coverDir, fileName);
            await File.WriteAllBytesAsync(filePath, bytes, cancellationToken);

            // Return local URL served by controller
            return $"/assets/coverart/{playlistId}/{Uri.EscapeDataString(trackId)}";
        }
        catch
        {
            return null;
        }
    }

    private static string SanitizeFileName(string input)
    {
        var invalid = IOPath.GetInvalidFileNameChars();
        return new string(input.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
    }

    private static string? GetExtensionFromContentType(MediaTypeHeaderValue? contentType)
    {
        var mediaType = contentType?.MediaType?.ToLowerInvariant();
        return mediaType switch
        {
            "image/jpeg" => ".jpg",
            "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            _ => null,
        };
    }

    private static string? GetExtensionFromUrl(string url)
    {
        var match = FileExtensionRegex.Match(url);
        if (match.Success)
            return "." + match.Groups[1].Value.ToLowerInvariant();
        return null;
    }

    public (FileStream? Stream, string? ContentType, string? FileName) TryOpenCoverImage(
        Guid playlistId,
        string trackId
    )
    {
        var coverDir = IOPath.Combine(BaseDirectory, "coverart", playlistId.ToString());
        // Try multiple common extensions
        foreach (var ext in new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" })
        {
            var filePath = IOPath.Combine(coverDir, SanitizeFileName(trackId) + ext);
            if (File.Exists(filePath))
            {
                var stream = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read
                );
                var contentType = ext switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".webp" => "image/webp",
                    ".gif" => "image/gif",
                    _ => "application/octet-stream",
                };
                var fileName = IOPath.GetFileName(filePath);
                return (stream, contentType, fileName);
            }
        }

        return (null, null, null);
    }
}
