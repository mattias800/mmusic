using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ServerLibrary.Utils;
using Path = System.IO.Path;

namespace MusicGQL.Features.Artists;

/// <summary>
/// GraphQL type for artist appearances on other releases
/// </summary>
public record ArtistAppearsOnRelease(
    [property: GraphQLIgnore] JsonArtistAppearance Model,
    [property: GraphQLIgnore] string ArtistId
)
{
    /// <summary>
    /// Title of the release/album
    /// </summary>
    public string ReleaseTitle() => Model.ReleaseTitle;

    /// <summary>
    /// Type of release (Album, EP, Single)
    /// </summary>
    public string ReleaseType() => Model.ReleaseType;

    /// <summary>
    /// Name of the primary artist for this release
    /// </summary>
    public string PrimaryArtistName() => Model.PrimaryArtistName;

    /// <summary>
    /// MusicBrainz ID of the primary artist
    /// </summary>
    public string? PrimaryArtistMusicBrainzId() => Model.PrimaryArtistMusicBrainzId;

    /// <summary>
    /// MusicBrainz ID of the release group
    /// </summary>
    public string? MusicBrainzReleaseGroupId() => Model.MusicBrainzReleaseGroupId;

    /// <summary>
    /// First release date of the release group
    /// </summary>
    public string? FirstReleaseDate() => Model.FirstReleaseDate;

    /// <summary>
    /// Year of first release
    /// </summary>
    public string? FirstReleaseYear() => Model.FirstReleaseYear;

    /// <summary>
    /// Role of this artist on the release (e.g., "Featured Artist", "Producer", "Composer")
    /// </summary>
    public string? Role() => Model.Role;

    /// <summary>
    /// Gets the cover art URL that the server can serve
    /// </summary>
    public string? CoverArtUrl()
    {
        // If there's no cover art stored locally, return null
        if (string.IsNullOrEmpty(Model.CoverArt))
            return null;

        // Extract the appearance ID from the filename (e.g., "appearance_abc123_def456_cover.jpg" -> "abc123_def456")
        var fileName = Path.GetFileName(Model.CoverArt);
        if (string.IsNullOrEmpty(fileName) || !fileName.StartsWith("appearance_"))
            return null;

        // Remove "appearance_" prefix and "_cover.jpg" suffix
        var appearanceId = fileName
            .Replace("appearance_", "")
            .Replace("_cover.jpg", "")
            .Replace("_cover.png", "")
            .Replace("_cover.jpeg", "")
            .Replace("_cover.gif", "")
            .Replace("_cover.webp", "");

        if (string.IsNullOrEmpty(appearanceId))
            return null;

        return LibraryAssetUrlFactory.CreateAppearanceCoverArtUrl(ArtistId, appearanceId);
    }
}
