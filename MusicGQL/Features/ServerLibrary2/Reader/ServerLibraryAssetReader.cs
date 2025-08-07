using System.IO;

namespace MusicGQL.Features.ServerLibrary2.Reader;

/// <summary>
/// Reads asset files (images, audio) from the music library folder structure
/// </summary>
public class ServerLibraryAssetReader
{
    private const string LibraryPath = "./Library/";

    /// <summary>
    /// Gets an artist photo by artist ID and photo index
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <param name="photoIndex">Photo index (0-based)</param>
    /// <returns>File stream and content type, or null if not found</returns>
    public async Task<(Stream? stream, string? contentType, string? fileName)> GetArtistPhotoAsync(
        string artistId, 
        int photoIndex
    )
    {
        try
        {
            var artistPath = Path.Combine(LibraryPath, artistId);
            if (!Directory.Exists(artistPath))
                return (null, null, null);

            // Read artist.json to get photo references
            var artistJsonPath = Path.Combine(artistPath, "artist.json");
            if (!File.Exists(artistJsonPath))
                return (null, null, null);

            var jsonContent = await File.ReadAllTextAsync(artistJsonPath);
            var artistJson = System.Text.Json.JsonSerializer.Deserialize<Json.ArtistJson>(jsonContent, GetJsonOptions());

            if (artistJson?.Photos?.ThumbPhotos == null || photoIndex >= artistJson.Photos.ThumbPhotos.Count)
                return (null, null, null);

            var photoPath = artistJson.Photos.ThumbPhotos[photoIndex];
            
            // Handle relative paths (remove ./ prefix)
            if (photoPath.StartsWith("./"))
                photoPath = photoPath[2..];

            var fullPhotoPath = Path.Combine(artistPath, photoPath);
            
            if (!File.Exists(fullPhotoPath))
                return (null, null, null);

            var fileStream = new FileStream(fullPhotoPath, FileMode.Open, FileAccess.Read);
            var contentType = GetContentTypeFromExtension(Path.GetExtension(fullPhotoPath));
            var fileName = Path.GetFileName(fullPhotoPath);

            return (fileStream, contentType, fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading artist photo: {ex.Message}");
            return (null, null, null);
        }
    }

    /// <summary>
    /// Gets a release cover art by artist ID and release folder name
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <param name="releaseFolderName">Release folder name</param>
    /// <returns>File stream and content type, or null if not found</returns>
    public async Task<(Stream? stream, string? contentType, string? fileName)> GetReleaseCoverArtAsync(
        string artistId,
        string releaseFolderName
    )
    {
        try
        {
            var releasePath = Path.Combine(LibraryPath, artistId, releaseFolderName);
            if (!Directory.Exists(releasePath))
                return (null, null, null);

            // Read release.json to get cover art references
            var releaseJsonPath = Path.Combine(releasePath, "release.json");
            if (!File.Exists(releaseJsonPath))
                return (null, null, null);

            var jsonContent = await File.ReadAllTextAsync(releaseJsonPath);
            var releaseJson = System.Text.Json.JsonSerializer.Deserialize<Json.ReleaseJson>(jsonContent, GetJsonOptions());

            // Try to get cover art from release.json
            string? coverArtPath = null;
            
            // Check if release.json has cover art reference (you may need to add this to ReleaseJson model)
            // For now, look for common cover art file names
            var commonCoverNames = new[] { "cover.jpg", "cover.png", "cover.jpeg", "folder.jpg", "folder.png" };
            
            foreach (var coverName in commonCoverNames)
            {
                var potentialPath = Path.Combine(releasePath, coverName);
                if (File.Exists(potentialPath))
                {
                    coverArtPath = coverName;
                    break;
                }
            }

            if (string.IsNullOrEmpty(coverArtPath))
                return (null, null, null);

            var fullCoverPath = Path.Combine(releasePath, coverArtPath);
            
            if (!File.Exists(fullCoverPath))
                return (null, null, null);

            var fileStream = new FileStream(fullCoverPath, FileMode.Open, FileAccess.Read);
            var contentType = GetContentTypeFromExtension(Path.GetExtension(fullCoverPath));
            var fileName = Path.GetFileName(fullCoverPath);

            return (fileStream, contentType, fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading release cover art: {ex.Message}");
            return (null, null, null);
        }
    }

    /// <summary>
    /// Gets an audio file by artist ID, release folder name, and track number
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <param name="releaseFolderName">Release folder name</param>
    /// <param name="trackNumber">Track number</param>
    /// <returns>File stream and content type, or null if not found</returns>
    public async Task<(Stream? stream, string? contentType, string? fileName)> GetTrackAudioAsync(
        string artistId,
        string releaseFolderName,
        int trackNumber
    )
    {
        try
        {
            var releasePath = Path.Combine(LibraryPath, artistId, releaseFolderName);
            if (!Directory.Exists(releasePath))
                return (null, null, null);

            // Read release.json to get track references
            var releaseJsonPath = Path.Combine(releasePath, "release.json");
            if (!File.Exists(releaseJsonPath))
                return (null, null, null);

            var jsonContent = await File.ReadAllTextAsync(releaseJsonPath);
            var releaseJson = System.Text.Json.JsonSerializer.Deserialize<Json.ReleaseJson>(jsonContent, GetJsonOptions());

            if (releaseJson?.Tracks == null)
                return (null, null, null);

            var track = releaseJson.Tracks.FirstOrDefault(t => t.TrackNumber == trackNumber);
            if (track?.AudioFilePath == null)
                return (null, null, null);

            // Handle relative paths
            var audioPath = track.AudioFilePath;
            if (audioPath.StartsWith("./"))
                audioPath = audioPath[2..];

            var fullAudioPath = Path.Combine(releasePath, audioPath);
            
            if (!File.Exists(fullAudioPath))
                return (null, null, null);

            var fileStream = new FileStream(fullAudioPath, FileMode.Open, FileAccess.Read);
            var contentType = GetContentTypeFromExtension(Path.GetExtension(fullAudioPath));
            var fileName = Path.GetFileName(fullAudioPath);

            return (fileStream, contentType, fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading track audio: {ex.Message}");
            return (null, null, null);
        }
    }

    /// <summary>
    /// Gets the appropriate content type for a file extension
    /// </summary>
    private static string GetContentTypeFromExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".flac" => "audio/flac",
            ".m4a" => "audio/mp4",
            ".ogg" => "audio/ogg",
            _ => "application/octet-stream"
        };
    }

    /// <summary>
    /// Gets JSON serializer options consistent with ServerLibraryJsonReader
    /// </summary>
    private static System.Text.Json.JsonSerializerOptions GetJsonOptions()
    {
        return new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase) }
        };
    }
} 