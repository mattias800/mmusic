using MusicGQL.Features.ServerSettings;
using Path = System.IO.Path;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MusicGQL.Features.ServerLibrary.Reader;

/// <summary>
/// Reads asset files (images, audio) from the music library folder structure
/// </summary>
public class ServerLibraryAssetReader(ServerSettingsAccessor serverSettingsAccessor)
{
    private async Task<string> GetLibraryPathAsync()
    {
        try
        {
            var s = await serverSettingsAccessor.GetAsync();
            return s.LibraryPath;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets an artist photo by artist ID, photo type, and photo index
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <param name="photoType">Photo type (thumbs, backgrounds, banners, logos)</param>
    /// <param name="photoIndex">Photo index (0-based)</param>
    /// <returns>File stream and content type, or null if not found</returns>
    public async Task<(Stream? stream, string? contentType, string? fileName)> GetArtistPhotoAsync(
        string artistId,
        string photoType,
        int photoIndex
    )
    {
        try
        {
            var libraryPath = await GetLibraryPathAsync();
            var artistPath = Path.Combine(libraryPath, artistId);
            if (!Directory.Exists(artistPath))
                return (null, null, null);

            // Read artist.json to get photo references (strict)
            var artistJsonPath = Path.Combine(artistPath, "artist.json");
            if (!File.Exists(artistJsonPath))
                return (null, null, null);

            var jsonContent = await File.ReadAllTextAsync(artistJsonPath);
            var artistJson = JsonSerializer.Deserialize<Json.JsonArtist>(
                jsonContent,
                GetJsonOptions()
            );

            // Get the appropriate photo list based on type (if available)
            if (artistJson?.Photos == null)
                return (null, null, null);

            List<string>? photoList = photoType.ToLowerInvariant() switch
                {
                    "thumbs" => artistJson.Photos.Thumbs,
                    "backgrounds" => artistJson.Photos.Backgrounds,
                    "banners" => artistJson.Photos.Banners,
                    "logos" => artistJson.Photos.Logos,
                    _ => null,
                };

            if (photoList == null || photoIndex >= photoList.Count)
                return (null, null, null);

            var photoPath = photoList[photoIndex];

            if (string.IsNullOrEmpty(photoPath))
                return (null, null, null);

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
    /// Gets an artist thumbnail by artist ID and photo index (backward compatibility)
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <param name="photoIndex">Photo index (0-based)</param>
    /// <returns>File stream and content type, or null if not found</returns>
    public async Task<(Stream? stream, string? contentType, string? fileName)> GetArtistPhotoAsync(
        string artistId,
        int photoIndex
    )
    {
        return await GetArtistPhotoAsync(artistId, "thumbs", photoIndex);
    }

    /// <summary>
    /// Gets a release cover art by artist ID and release folder name
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <param name="releaseFolderName">Release folder name</param>
    /// <returns>File stream and content type, or null if not found</returns>
    public async Task<(
        Stream? stream,
        string? contentType,
        string? fileName
    )> GetReleaseCoverArtAsync(string artistId, string releaseFolderName)
    {
        try
        {
            var libraryPath = await GetLibraryPathAsync();
            var releasePath = Path.Combine(libraryPath, artistId, releaseFolderName);
            if (!Directory.Exists(releasePath))
                return (null, null, null);

            // Read release.json to get cover art references (strict)
            var releaseJsonPath = Path.Combine(releasePath, "release.json");
            if (!File.Exists(releaseJsonPath))
                return (null, null, null);

            var jsonContent = await File.ReadAllTextAsync(releaseJsonPath);
            var releaseJson = JsonSerializer.Deserialize<Json.JsonRelease>(
                jsonContent,
                GetJsonOptions()
            );

            // Get cover art path from release.json only
            string? coverArtPath = releaseJson?.CoverArt;
            if (!string.IsNullOrEmpty(coverArtPath) && coverArtPath.StartsWith("./"))
                coverArtPath = coverArtPath[2..];

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
    /// Gets a top track cover art by artist ID and index.
    /// Reads artist.json and if the top track has a mapped release, serves that release cover art;
    /// otherwise serves the local toptrackNN.jpg if present.
    /// </summary>
    public async Task<(Stream? stream, string? contentType, string? fileName)> GetTopTrackCoverArtAsync(
        string artistId,
        int index
    )
    {
        try
        {
            var libraryPath = await GetLibraryPathAsync();
            var artistPath = Path.Combine(libraryPath, artistId);
            if (!Directory.Exists(artistPath))
                return (null, null, null);

            var artistJsonPath = Path.Combine(artistPath, "artist.json");
            if (!File.Exists(artistJsonPath))
                return (null, null, null);

            var jsonContent = await File.ReadAllTextAsync(artistJsonPath);
            var artistJson = JsonSerializer.Deserialize<Json.JsonArtist>(
                jsonContent,
                GetJsonOptions()
            );

            if (artistJson?.TopTracks == null || index < 0 || index >= artistJson.TopTracks.Count)
                return (null, null, null);

            var topTrack = artistJson.TopTracks[index];

            // If mapped to a local release, defer to release.json cover art strictly
            if (!string.IsNullOrWhiteSpace(topTrack.ReleaseFolderName))
            {
                return await GetReleaseCoverArtAsync(artistId, topTrack.ReleaseFolderName);
            }

            // Otherwise, serve local toptrackNN.jpg referenced by CoverArt
            var coverArtRel = topTrack.CoverArt;

            if (string.IsNullOrEmpty(coverArtRel))
                return (null, null, null);

            if (coverArtRel.StartsWith("./"))
            {
                coverArtRel = coverArtRel[2..];
            }

            var fullPath = Path.Combine(artistPath, coverArtRel);
            if (!File.Exists(fullPath))
                return (null, null, null);

            var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            var contentType = GetContentTypeFromExtension(Path.GetExtension(fullPath));
            var fileName = Path.GetFileName(fullPath);
            return (fileStream, contentType, fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading top track cover art: {ex.Message}");
            return (null, null, null);
        }
    }

    /// <summary>
    /// Gets appearance cover art by artist ID and appearance ID
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <param name="appearanceId">Appearance ID (from the filename like "appearance_abc123_def456_cover.jpg")</param>
    /// <returns>File stream and content type, or null if not found</returns>
    public async Task<(Stream? stream, string? contentType, string? fileName)> GetAppearanceCoverArtAsync(
        string artistId,
        string appearanceId
    )
    {
        try
        {
            var libraryPath = await GetLibraryPathAsync();
            var artistPath = Path.Combine(libraryPath, artistId);
            if (!Directory.Exists(artistPath))
                return (null, null, null);

            // Look for appearance cover art files in the artist folder
            var appearanceFiles = Directory
                .GetFiles(artistPath, $"appearance_{appearanceId}_cover.*")
                .Where(f => IsImageFile(f))
                .ToList();

            if (appearanceFiles.Count == 0)
                return (null, null, null);

            // Use the first found appearance cover art file
            var coverArtPath = appearanceFiles[0];
            var fileStream = new FileStream(coverArtPath, FileMode.Open, FileAccess.Read);
            var contentType = GetContentTypeFromExtension(Path.GetExtension(coverArtPath));
            var fileName = Path.GetFileName(coverArtPath);

            return (fileStream, contentType, fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading appearance cover art: {ex.Message}");
            return (null, null, null);
        }
    }

    /// <summary>
    /// Gets a similar artist thumbnail by parent artist and similar MBID
    /// Looks for files named similar_thumb_{mbid}.* in the artist directory
    /// </summary>
    public async Task<(Stream? stream, string? contentType, string? fileName)> GetSimilarArtistThumbAsync(
        string artistId,
        string musicBrainzArtistId
    )
    {
        try
        {
            var libraryPath = await GetLibraryPathAsync();
            var artistPath = Path.Combine(libraryPath, artistId);
            if (!Directory.Exists(artistPath))
                return (null, null, null);

            var pattern = $"similar_thumb_{musicBrainzArtistId}.*";
            var matches = Directory
                .GetFiles(artistPath, pattern)
                .Where(f => IsImageFile(f))
                .ToList();

            if (matches.Count == 0)
                return (null, null, null);

            var filePath = matches[0];
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var contentType = GetContentTypeFromExtension(Path.GetExtension(filePath));
            var fileName = Path.GetFileName(filePath);
            return (fileStream, contentType, fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading similar artist thumb: {ex.Message}");
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
            var libraryPath = await GetLibraryPathAsync();
            var releasePath = Path.Combine(libraryPath, artistId, releaseFolderName);
            if (!Directory.Exists(releasePath))
                return (null, null, null);

            // Read release.json to get track references
            var releaseJsonPath = Path.Combine(releasePath, "release.json");
            if (!File.Exists(releaseJsonPath))
                return (null, null, null);

            var jsonContent = await File.ReadAllTextAsync(releaseJsonPath);
            var releaseJson = JsonSerializer.Deserialize<Json.JsonRelease>(
                jsonContent,
                GetJsonOptions()
            );

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
            _ => "application/octet-stream",
        };
    }

    /// <summary>
    /// Gets JSON serialization options
    /// </summary>
    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };
    }

    /// <summary>
    /// Checks if a file is an image file based on its extension
    /// </summary>
    private static bool IsImageFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => true,
            _ => false
        };
    }
}
