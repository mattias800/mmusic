using System.Net.Http.Headers;
using MusicGQL.Features.ServerLibrary.Utils;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Integration.Spotify;
using Path = System.IO.Path;
using MusicGQL.Features.ServerSettings;

namespace MusicGQL.Features.Import.Services;

/// <summary>
/// Coordinator for downloading images: tries fanart.tv first, then falls back to
/// Cover Art Archive and Spotify where applicable.
/// </summary>
public class CoverArtDownloadService(
    FanArtDownloadService fanArtService,
    MusicBrainzService musicBrainzService,
    SpotifyService spotifyService,
    HttpClient httpClient
)
{
    /// <summary>
    /// Downloads artist photos (thumbs/backgrounds/etc.) using fanart.tv, with Spotify image fallback for thumbs.
    /// </summary>
    public async Task<FanArtDownloadResult> DownloadArtistPhotosAsync(
        string musicBrainzId,
        string artistFolderPath
    )
    {
        // Guard: require library manifest present (safety)
        try
        {
            var libRoot = Directory.GetParent(artistFolderPath)?.FullName ?? string.Empty;
            if (!File.Exists(Path.Combine(libRoot, LibraryManifestService.ManifestFileName)))
            {
                return new FanArtDownloadResult();
            }
        }
        catch { return new FanArtDownloadResult(); }

        var result = await fanArtService.DownloadArtistPhotosAsync(
            musicBrainzId,
            artistFolderPath
        );

        // Spotify fallback(s) when fanart.tv misses
        try
        {
            if (result.Thumbs.Count == 0 || result.Backgrounds.Count == 0)
            {
                var mbArtist = await musicBrainzService.GetArtistByIdAsync(musicBrainzId);
                var artistName = mbArtist?.Name;
                if (!string.IsNullOrWhiteSpace(artistName))
                {
                    var candidates = await spotifyService.SearchArtistsAsync(artistName!, 1);
                    var best = candidates?.FirstOrDefault();
                    var imageUrl = best?.Images?.FirstOrDefault()?.Url;
                    if (!string.IsNullOrWhiteSpace(imageUrl))
                    {
                        var ext = GetExtensionFromUrlOrDefault(imageUrl!, ".jpg");

                        if (result.Thumbs.Count == 0)
                        {
                            var thumbFile = $"thumb_00{ext}";
                            var thumbPath = Path.Combine(artistFolderPath, thumbFile);
                            var bytes = await httpClient.GetByteArrayAsync(imageUrl!);
                            await File.WriteAllBytesAsync(thumbPath, bytes);
                            result.Thumbs.Add("./" + thumbFile);
                        }

                        if (result.Backgrounds.Count == 0)
                        {
                            var bgFile = $"background_00{ext}";
                            var bgPath = Path.Combine(artistFolderPath, bgFile);
                            var bytes = await httpClient.GetByteArrayAsync(imageUrl!);
                            await File.WriteAllBytesAsync(bgPath, bytes);
                            result.Backgrounds.Add("./" + bgFile);
                        }
                    }
                }
            }
        }
        catch { }

        // If no banner but we have a background, reuse the background as a banner placeholder
        if (result.Banners.Count == 0 && result.Backgrounds.Count > 0)
        {
            result.Banners.Add(result.Backgrounds[0]);
        }

        return result;
    }

    /// <summary>
    /// Downloads release cover art with fallbacks: fanart.tv -> Cover Art Archive -> Spotify.
    /// </summary>
    public async Task<string?> DownloadReleaseCoverArtAsync(
        string releaseGroupId,
        string releaseFolderPath
    )
    {
        // Guard: require library manifest present (safety)
        try
        {
            // releaseFolderPath => <Library>/<Artist>/<Release>
            var libRoot = Directory.GetParent(Directory.GetParent(releaseFolderPath)?.FullName ?? string.Empty)?.FullName
                           ?? string.Empty;
            if (!File.Exists(Path.Combine(libRoot, LibraryManifestService.ManifestFileName)))
            {
                return null;
            }
        }
        catch { return null; }

        // 1) Try fanart.tv
        var viaFanArt = await fanArtService.DownloadReleaseCoverArtAsync(
            releaseGroupId,
            releaseFolderPath
        );
        if (!string.IsNullOrWhiteSpace(viaFanArt))
            return viaFanArt;

        // 2) Try Cover Art Archive via MB release
        var viaCaa = await TryDownloadCoverFromCoverArtArchiveAsync(
            releaseGroupId,
            releaseFolderPath
        );
        if (!string.IsNullOrWhiteSpace(viaCaa))
            return viaCaa;

        // 3) Try Spotify
        var viaSpotify = await TryDownloadCoverFromSpotifyAsync(
            releaseGroupId,
            releaseFolderPath
        );
        if (!string.IsNullOrWhiteSpace(viaSpotify))
            return viaSpotify;

        return null;
    }

    private async Task<string?> TryDownloadCoverFromCoverArtArchiveAsync(
        string releaseGroupId,
        string releaseFolderPath
    )
    {
        try
        {
            var releases = await musicBrainzService.GetReleasesForReleaseGroupAsync(releaseGroupId);
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
                    using var response = await httpClient.GetAsync(url);
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
            var releases = await musicBrainzService.GetReleasesForReleaseGroupAsync(releaseGroupId);
            var best = LibraryDecider.GetMainReleaseInReleaseGroup(releases.ToList());
            var groupTitle = best?.ReleaseGroup?.Title;
            var artistName = best?.Credits?.FirstOrDefault()?.Artist?.Name;
            if (string.IsNullOrWhiteSpace(groupTitle) || string.IsNullOrWhiteSpace(artistName))
                return null;

            var spArtists = await spotifyService.SearchArtistsAsync(artistName!, 1);
            var spArtist = spArtists?.FirstOrDefault();
            if (spArtist == null)
                return null;

            var albums = await spotifyService.GetArtistAlbumsAsync(spArtist.Id);
            var match = albums
                ?.FirstOrDefault(a => string.Equals(a.Name, groupTitle, StringComparison.OrdinalIgnoreCase));
            match ??= albums?.OrderByDescending(a => a.ReleaseDate).FirstOrDefault();

            var imageUrl = match?.Images?.FirstOrDefault()?.Url;
            if (string.IsNullOrWhiteSpace(imageUrl))
                return null;

            var ext = GetExtensionFromUrlOrDefault(imageUrl!, ".jpg");
            var fileName = "cover" + ext;
            var filePath = Path.Combine(releaseFolderPath, fileName);
            var bytes = await httpClient.GetByteArrayAsync(imageUrl!);
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


