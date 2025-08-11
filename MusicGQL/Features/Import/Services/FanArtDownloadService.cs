using TrackSeries.FanArtTV.Client;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import.Services;

/// <summary>
/// Service for downloading artist photos from fanart.tv
/// </summary>
public class FanArtDownloadService
{
    private readonly IFanArtTVClient _fanArtClient;
    private readonly HttpClient _httpClient;

    public FanArtDownloadService(
        IFanArtTVClient fanArtClient,
        HttpClient httpClient
    )
    {
        _fanArtClient = fanArtClient;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Downloads artist photos from fanart.tv and saves them to the artist folder
    /// </summary>
    /// <param name="musicBrainzId">MusicBrainz artist ID</param>
    /// <param name="artistFolderPath">Local artist folder path</param>
    /// <returns>Downloaded photo information organized by type</returns>
    public async Task<FanArtDownloadResult> DownloadArtistPhotosAsync(
        string musicBrainzId,
        string artistFolderPath
    )
    {
        var result = new FanArtDownloadResult();

        try
        {
            var artistImages = await _fanArtClient.Music.GetArtistAsync(musicBrainzId);
            if (artistImages == null)
                return result;

            // Download thumbnails (artist thumbs)
            if (artistImages.ArtistThumb?.Any() == true)
            {
                result.Thumbs = await DownloadImagesAsync(
                    artistImages.ArtistThumb.Take(5), // Limit to 5 thumbs
                    artistFolderPath,
                    "thumb"
                );
            }

            // Download backgrounds
            if (artistImages.ArtistBackground?.Any() == true)
            {
                result.Backgrounds = await DownloadImagesAsync(
                    artistImages.ArtistBackground.Take(3), // Limit to 3 backgrounds
                    artistFolderPath,
                    "background"
                );
            }

            // Download banners
            if (artistImages.MusicBanner?.Any() == true)
            {
                result.Banners = await DownloadImagesAsync(
                    artistImages.MusicBanner.Take(3), // Limit to 3 banners
                    artistFolderPath,
                    "banner"
                );
            }

            // Download logos
            if (artistImages.MusicLogo?.Any() == true)
            {
                result.Logos = await DownloadImagesAsync(
                    artistImages.MusicLogo.Take(3), // Limit to 3 logos
                    artistFolderPath,
                    "logo"
                );
            }

            // Also try HD logos
            if (artistImages.HDMusicLogo?.Any() == true)
            {
                var hdLogos = await DownloadImagesAsync(
                    artistImages.HDMusicLogo.Take(2), // Limit to 2 HD logos
                    artistFolderPath,
                    "logo_hd"
                );
                result.Logos.AddRange(hdLogos);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Error downloading fanart for artist '{musicBrainzId}': {ex.Message}"
            );
        }

        return result;
    }

    /// <summary>
    /// Downloads a collection of images from URLs
    /// </summary>
    /// <param name="images">Image objects with URLs</param>
    /// <param name="folderPath">Destination folder path</param>
    /// <param name="filePrefix">File name prefix</param>
    /// <returns>List of downloaded file paths relative to artist folder</returns>
    private async Task<List<string>> DownloadImagesAsync(
        IEnumerable<dynamic> images,
        string folderPath,
        string filePrefix
    )
    {
        var downloadedFiles = new List<string>();
        var index = 0;

        foreach (var image in images)
        {
            try
            {
                string? imageUrl = image.Url;
                if (string.IsNullOrEmpty(imageUrl))
                    continue;

                // Determine file extension from URL
                var uri = new Uri(imageUrl);
                var extension = Path.GetExtension(uri.LocalPath);
                if (string.IsNullOrEmpty(extension))
                {
                    extension = ".jpg"; // Default to .jpg if no extension found
                }

                var fileName = $"{filePrefix}_{index:D2}{extension}";
                var filePath = Path.Combine(folderPath, fileName);

                // Download the image
                var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
                await File.WriteAllBytesAsync(filePath, imageBytes);

                // Add relative path (just the filename) to the result
                downloadedFiles.Add($"./{fileName}");

                Console.WriteLine($"Downloaded {filePrefix}: {fileName}");
                index++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading {filePrefix} image: {ex.Message}");
            }
        }

        return downloadedFiles;
    }

    /// <summary>
    /// Downloads cover art for a release
    /// </summary>
    /// <param name="releaseGroupId">MusicBrainz release group ID</param>
    /// <param name="releaseFolderPath">Local release folder path</param>
    /// <returns>Downloaded cover art file path or null</returns>
    public async Task<string?> DownloadReleaseCoverArtAsync(
        string releaseGroupId,
        string releaseFolderPath
    )
    {
        try
        {
            var artistInfo = await _fanArtClient.Music.GetAlbumAsync(releaseGroupId);

            var albumImages = artistInfo.Albums.GetValueOrDefault(Guid.Parse(releaseGroupId));

            if (albumImages?.AlbumCover?.Any() != true)
                return null;

            var coverImage = albumImages.AlbumCover.First();
            if (string.IsNullOrEmpty(coverImage.Url))
                return null;

            // Download the cover art
            var uri = new Uri(coverImage.Url);
            var extension = Path.GetExtension(uri.LocalPath);
            if (string.IsNullOrEmpty(extension))
            {
                extension = ".jpg";
            }

            var fileName = $"cover{extension}";
            var filePath = Path.Combine(releaseFolderPath, fileName);

            var imageBytes = await _httpClient.GetByteArrayAsync(coverImage.Url);
            await File.WriteAllBytesAsync(filePath, imageBytes);

            Console.WriteLine($"Downloaded cover art: {fileName}");
            return $"./{fileName}";
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Error downloading cover art for release group '{releaseGroupId}': {ex.Message}"
            );
            return null;
        }
    }
}

/// <summary>
/// Result of downloading fanart photos
/// </summary>
public class FanArtDownloadResult
{
    public List<string> Thumbs { get; set; } = [];
    public List<string> Backgrounds { get; set; } = [];
    public List<string> Banners { get; set; } = [];
    public List<string> Logos { get; set; } = [];
}
