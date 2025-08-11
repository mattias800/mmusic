using TrackSeries.FanArtTV.Client;
using MusicGQL.Integration.Spotify;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Features.ServerLibrary.Utils;
using System.Net.Http.Headers;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import.Services;

/// <summary>
/// Service for downloading artist photos from fanart.tv
/// </summary>
public class FanArtDownloadService
{
    private readonly IFanArtTVClient _fanArtClient;
    private readonly HttpClient _httpClient;
    private readonly SpotifyService _spotifyService;
    private readonly MusicBrainzService _musicBrainzService;

    public FanArtDownloadService(
        IFanArtTVClient fanArtClient,
        HttpClient httpClient,
        SpotifyService spotifyService,
        MusicBrainzService musicBrainzService
    )
    {
        _fanArtClient = fanArtClient;
        _httpClient = httpClient;
        _spotifyService = spotifyService;
        _musicBrainzService = musicBrainzService;
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

        // Fallback: Spotify artist image if no thumbs available
        try
        {
            if (result.Thumbs.Count == 0)
            {
                var mbArtist = await _musicBrainzService.GetArtistByIdAsync(musicBrainzId);
                var artistName = mbArtist?.Name;
                if (!string.IsNullOrWhiteSpace(artistName))
                {
                    var candidates = await _spotifyService.SearchArtistsAsync(artistName!, 1);
                    var best = candidates?.FirstOrDefault();
                    var imageUrl = best?.Images?.FirstOrDefault()?.Url;
                    if (!string.IsNullOrWhiteSpace(imageUrl))
                    {
                        var ext = GetExtensionFromUrlOrDefault(imageUrl!, ".jpg");
                        var fileName = $"thumb_00{ext}";
                        var filePath = Path.Combine(artistFolderPath, fileName);
                        var bytes = await _httpClient.GetByteArrayAsync(imageUrl!);
                        await File.WriteAllBytesAsync(filePath, bytes);
                        result.Thumbs.Add("./" + fileName);
                    }
                }
            }
        }
        catch { }

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
        // 1) Try fanart.tv by release group id
        try
        {
            var artistInfo = await _fanArtClient.Music.GetAlbumAsync(releaseGroupId);
            var albumImages = artistInfo.Albums.GetValueOrDefault(Guid.Parse(releaseGroupId));
            if (albumImages?.AlbumCover?.Any() == true)
            {
                var coverImage = albumImages.AlbumCover.First();
                if (!string.IsNullOrWhiteSpace(coverImage.Url))
                {
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
                    Console.WriteLine($"Downloaded cover art (fanart.tv): {fileName}");
                    return "./" + fileName;
                }
            }
        }
        catch { }

        // 2) Try Cover Art Archive using best release in the group
        var viaCaa = await TryDownloadCoverFromCoverArtArchiveAsync(
            releaseGroupId,
            releaseFolderPath
        );
        if (!string.IsNullOrWhiteSpace(viaCaa))
        {
            return viaCaa;
        }

        // 3) Final fallback: Spotify
        var viaSpotify = await TryDownloadCoverFromSpotifyAsync(
            releaseGroupId,
            releaseFolderPath
        );
        if (!string.IsNullOrWhiteSpace(viaSpotify))
        {
            return viaSpotify;
        }

        return null;
    }

    private async Task<string?> TryDownloadCoverFromCoverArtArchiveAsync(
        string releaseGroupId,
        string releaseFolderPath
    )
    {
        try
        {
            var releases = await _musicBrainzService.GetReleasesForReleaseGroupAsync(releaseGroupId);
            var best = LibraryDecider.GetMainReleaseInReleaseGroup(releases.ToList());
            if (best == null || string.IsNullOrWhiteSpace(best.Id))
                return null;

            var candidates = new List<string>
            {
                $"https://coverartarchive.org/release/{best.Id}/front-500.jpg",
                $"https://coverartarchive.org/release/{best.Id}/front",
                $"https://coverartarchive.org/release/{best.Id}/front.jpg",
            };

            foreach (var url in candidates)
            {
                try
                {
                    using var response = await _httpClient.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                        continue;
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    var ext = GetExtensionFromContentType(response.Content.Headers.ContentType)
                              ?? GetExtensionFromUrlOrDefault(url, ".jpg");
                    var fileName = "cover" + ext;
                    var filePath = Path.Combine(releaseFolderPath, fileName);
                    await File.WriteAllBytesAsync(filePath, bytes);
                    Console.WriteLine($"Downloaded cover art (CAA): {fileName}");
                    return "./" + fileName;
                }
                catch { }
            }
        }
        catch { }

        return null;
    }

    private async Task<string?> TryDownloadCoverFromSpotifyAsync(
        string releaseGroupId,
        string releaseFolderPath
    )
    {
        try
        {
            var releases = await _musicBrainzService.GetReleasesForReleaseGroupAsync(releaseGroupId);
            var best = LibraryDecider.GetMainReleaseInReleaseGroup(releases.ToList());
            var groupTitle = best?.ReleaseGroup?.Title;
            var artistName = best?.Credits?.FirstOrDefault()?.Artist?.Name;
            if (string.IsNullOrWhiteSpace(groupTitle) || string.IsNullOrWhiteSpace(artistName))
                return null;

            var spArtists = await _spotifyService.SearchArtistsAsync(artistName!, 1);
            var spArtist = spArtists?.FirstOrDefault();
            if (spArtist == null)
                return null;

            var albums = await _spotifyService.GetArtistAlbumsAsync(spArtist.Id);
            var match = albums
                ?.FirstOrDefault(a => string.Equals(a.Name, groupTitle, StringComparison.OrdinalIgnoreCase));
            match ??= albums?.OrderByDescending(a => a.ReleaseDate).FirstOrDefault();

            var imageUrl = match?.Images?.FirstOrDefault()?.Url;
            if (string.IsNullOrWhiteSpace(imageUrl))
                return null;

            var ext = GetExtensionFromUrlOrDefault(imageUrl!, ".jpg");
            var fileName = "cover" + ext;
            var filePath = Path.Combine(releaseFolderPath, fileName);
            var bytes = await _httpClient.GetByteArrayAsync(imageUrl!);
            await File.WriteAllBytesAsync(filePath, bytes);
            Console.WriteLine($"Downloaded cover art (Spotify): {fileName}");
            return "./" + fileName;
        }
        catch { }

        return null;
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

    private static string GetExtensionFromUrlOrDefault(string url, string defaultExt)
    {
        try
        {
            var uri = new Uri(url);
            var ext = Path.GetExtension(uri.LocalPath);
            if (string.IsNullOrWhiteSpace(ext))
                return defaultExt;
            return ext;
        }
        catch
        {
            return defaultExt;
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
