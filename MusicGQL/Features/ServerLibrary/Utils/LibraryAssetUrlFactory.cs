using MusicGQL.Features.ServerLibrary.Json;

namespace MusicGQL.Features.ServerLibrary.Utils;

/// <summary>
/// Factory for generating library asset URLs
/// </summary>
public static class LibraryAssetUrlFactory
{
    /// <summary>
    /// Creates a list of artist photo URLs for a specific photo type
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <param name="photoType">Photo type (thumbs, backgrounds, banners, logos)</param>
    /// <param name="photoCount">Number of photos</param>
    /// <returns>List of photo URLs</returns>
    public static List<string> CreateArtistPhotoUrls(
        string artistId,
        string photoType,
        int photoCount
    )
    {
        if (photoCount <= 0)
            return [];

        var escapedArtistId = Uri.EscapeDataString(artistId);
        return
        [
            .. Enumerable
                .Range(0, photoCount)
                .Select(index => $"/library/{escapedArtistId}/photos/{photoType}/{index}"),
        ];
    }

    /// <summary>
    /// Creates artist photo URLs for all available photo types
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <param name="photos">Artist photos from JSON</param>
    /// <returns>Dictionary of photo type to URLs</returns>
    public static Dictionary<string, List<string>> CreateAllArtistPhotoUrls(
        string artistId,
        JsonArtistPhotos? photos
    )
    {
        if (photos == null)
            return [];

        return new Dictionary<string, List<string>>
        {
            ["thumbs"] = CreateArtistPhotoUrls(artistId, "thumbs", photos.Thumbs?.Count ?? 0),
            ["backgrounds"] = CreateArtistPhotoUrls(
                artistId,
                "backgrounds",
                photos.Backgrounds?.Count ?? 0
            ),
            ["banners"] = CreateArtistPhotoUrls(artistId, "banners", photos.Banners?.Count ?? 0),
            ["logos"] = CreateArtistPhotoUrls(artistId, "logos", photos.Logos?.Count ?? 0),
        };
    }

    /// <summary>
    /// Creates an artist thumbnail photo URL
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <param name="photoIndex">Photo index</param>
    /// <returns>Thumbnail photo URL</returns>
    public static string CreateArtistThumbnailUrl(string artistId, int photoIndex = 0)
    {
        var escapedArtistId = Uri.EscapeDataString(artistId);
        return $"/library/{escapedArtistId}/photos/thumbs/{photoIndex}";
    }

    /// <summary>
    /// Creates an artist background photo URL
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <param name="photoIndex">Photo index</param>
    /// <returns>Background photo URL</returns>
    public static string CreateArtistBackgroundUrl(string artistId, int photoIndex = 0)
    {
        var escapedArtistId = Uri.EscapeDataString(artistId);
        return $"/library/{escapedArtistId}/photos/backgrounds/{photoIndex}";
    }

    /// <summary>
    /// Creates an artist banner photo URL
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <param name="photoIndex">Photo index</param>
    /// <returns>Banner photo URL</returns>
    public static string CreateArtistBannerUrl(string artistId, int photoIndex = 0)
    {
        var escapedArtistId = Uri.EscapeDataString(artistId);
        return $"/library/{escapedArtistId}/photos/banners/{photoIndex}";
    }

    /// <summary>
    /// Creates an artist logo photo URL
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <param name="photoIndex">Photo index</param>
    /// <returns>Logo photo URL</returns>
    public static string CreateArtistLogoUrl(string artistId, int photoIndex = 0)
    {
        var escapedArtistId = Uri.EscapeDataString(artistId);
        return $"/library/{escapedArtistId}/photos/logos/{photoIndex}";
    }

    /// <summary>
    /// Creates a release cover art URL
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <param name="releaseFolderName">Release folder name</param>
    /// <returns>Cover art URL</returns>
    public static string CreateReleaseCoverArtUrl(string artistId, string releaseFolderName)
    {
        var escapedArtistId = Uri.EscapeDataString(artistId);
        var escapedReleaseFolderName = Uri.EscapeDataString(releaseFolderName);
        return $"/library/{escapedArtistId}/releases/{escapedReleaseFolderName}/coverart";
    }

    /// <summary>
    /// Creates a track audio URL
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <param name="releaseFolderName">Release folder name</param>
    /// <param name="trackNumber">Track number</param>
    /// <returns>Track audio URL</returns>
    public static string CreateTrackAudioUrl(
        string artistId,
        string releaseFolderName,
        int trackNumber
    )
    {
        var escapedArtistId = Uri.EscapeDataString(artistId);
        var escapedReleaseFolderName = Uri.EscapeDataString(releaseFolderName);
        return $"/library/{escapedArtistId}/releases/{escapedReleaseFolderName}/tracks/{trackNumber}/audio";
    }

    /// <summary>
    /// Creates an appearance cover art URL
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <param name="appearanceId">Appearance ID (from the filename like "appearance_abc123_def456_cover.jpg")</param>
    /// <returns>Appearance cover art URL</returns>
    public static string CreateAppearanceCoverArtUrl(string artistId, string appearanceId)
    {
        var escapedArtistId = Uri.EscapeDataString(artistId);
        var escapedAppearanceId = Uri.EscapeDataString(appearanceId);
        return $"/library/{escapedArtistId}/appearances/{escapedAppearanceId}/coverart";
    }
}
