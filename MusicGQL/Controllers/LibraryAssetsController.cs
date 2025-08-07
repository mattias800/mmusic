using Microsoft.AspNetCore.Mvc;
using MusicGQL.Features.ServerLibrary.Reader;

namespace MusicGQL.Controllers;

/// <summary>
/// Controller for serving music library assets (images, audio files)
/// </summary>
[ApiController]
[Route("library")]
public class LibraryAssetsController : ControllerBase
{
    private readonly ServerLibraryAssetReader _assetReader;

    public LibraryAssetsController(ServerLibraryAssetReader assetReader)
    {
        _assetReader = assetReader;
    }

    /// <summary>
    /// Serves artist photos by type
    /// GET /library/{artistId}/photos/{photoType}/{photoIndex}
    /// </summary>
    [HttpGet("{artistId}/photos/{photoType}/{photoIndex:int}")]
    public async Task<IActionResult> GetArtistPhoto(string artistId, string photoType, int photoIndex)
    {
        // Validate photo type
        var validPhotoTypes = new[] { "thumbs", "backgrounds", "banners", "logos" };
        if (!validPhotoTypes.Contains(photoType.ToLowerInvariant()))
        {
            return BadRequest($"Invalid photo type '{photoType}'. Valid types are: {string.Join(", ", validPhotoTypes)}");
        }

        var (stream, contentType, fileName) = await _assetReader.GetArtistPhotoAsync(
            artistId,
            photoType,
            photoIndex
        );

        if (stream == null || contentType == null)
        {
            return NotFound(
                $"Artist {photoType} photo not found for artist '{artistId}' at index {photoIndex}"
            );
        }

        // Determine if we should include file extension in response
        var shouldIncludeExtension = ShouldIncludeFileExtension(Request.Path);

        return File(stream, contentType, shouldIncludeExtension ? fileName : null);
    }

    /// <summary>
    /// Serves artist thumbnail photos (backward compatibility)
    /// GET /library/{artistId}/photos/thumbs/{photoIndex}
    /// </summary>
    [HttpGet("{artistId}/photos/thumbs/{photoIndex:int}")]
    public async Task<IActionResult> GetArtistThumbnail(string artistId, int photoIndex)
    {
        return await GetArtistPhoto(artistId, "thumbs", photoIndex);
    }

    /// <summary>
    /// Serves release cover art
    /// GET /library/{artistId}/releases/{releaseFolderName}/coverart
    /// </summary>
    [HttpGet("{artistId}/releases/{releaseFolderName}/coverart")]
    public async Task<IActionResult> GetReleaseCoverArt(string artistId, string releaseFolderName)
    {
        var (stream, contentType, fileName) = await _assetReader.GetReleaseCoverArtAsync(
            artistId,
            releaseFolderName
        );

        if (stream == null || contentType == null)
        {
            return NotFound(
                $"Cover art not found for release '{releaseFolderName}' by artist '{artistId}'"
            );
        }

        // Determine if we should include file extension in response
        var shouldIncludeExtension = ShouldIncludeFileExtension(Request.Path);

        return File(stream, contentType, shouldIncludeExtension ? fileName : null);
    }

    /// <summary>
    /// Serves track audio files
    /// GET /library/{artistId}/releases/{releaseFolderName}/tracks/{trackNumber}/audio
    /// </summary>
    [HttpGet("{artistId}/releases/{releaseFolderName}/tracks/{trackNumber:int}/audio")]
    public async Task<IActionResult> GetTrackAudio(
        string artistId,
        string releaseFolderName,
        int trackNumber
    )
    {
        var (stream, contentType, fileName) = await _assetReader.GetTrackAudioAsync(
            artistId,
            releaseFolderName,
            trackNumber
        );

        if (stream == null || contentType == null)
        {
            return NotFound(
                $"Audio file not found for track {trackNumber} in release '{releaseFolderName}' by artist '{artistId}'"
            );
        }

        // For audio files, we typically want to include the filename for download purposes
        return File(stream, contentType, fileName);
    }

    /// <summary>
    /// Alternative endpoint with file extension for artist photos by type
    /// GET /library/{artistId}/photos/{photoType}/{photoIndex}.{extension}
    /// </summary>
    [HttpGet("{artistId}/photos/{photoType}/{photoIndex:int}.{extension}")]
    public async Task<IActionResult> GetArtistPhotoWithExtension(
        string artistId,
        string photoType,
        int photoIndex,
        string extension
    )
    {
        // Validate photo type
        var validPhotoTypes = new[] { "thumbs", "backgrounds", "banners", "logos" };
        if (!validPhotoTypes.Contains(photoType.ToLowerInvariant()))
        {
            return BadRequest($"Invalid photo type '{photoType}'. Valid types are: {string.Join(", ", validPhotoTypes)}");
        }

        var (stream, contentType, fileName) = await _assetReader.GetArtistPhotoAsync(
            artistId,
            photoType,
            photoIndex
        );

        if (stream == null || contentType == null)
        {
            return NotFound(
                $"Artist {photoType} photo not found for artist '{artistId}' at index {photoIndex}"
            );
        }

        // Validate that the requested extension matches the actual file
        var actualExtension = System
            .IO.Path.GetExtension(fileName)
            ?.TrimStart('.')
            .ToLowerInvariant();
        if (actualExtension != extension.ToLowerInvariant())
        {
            return NotFound(
                $"Requested extension '{extension}' does not match actual file extension '{actualExtension}'"
            );
        }

        return File(stream, contentType, fileName);
    }

    /// <summary>
    /// Alternative endpoint with file extension for artist thumbnail photos (backward compatibility)
    /// GET /library/{artistId}/photos/thumbs/{photoIndex}.{extension}
    /// </summary>
    [HttpGet("{artistId}/photos/thumbs/{photoIndex:int}.{extension}")]
    public async Task<IActionResult> GetArtistThumbnailWithExtension(
        string artistId,
        int photoIndex,
        string extension
    )
    {
        return await GetArtistPhotoWithExtension(artistId, "thumbs", photoIndex, extension);
    }

    /// <summary>
    /// Alternative endpoint with file extension for cover art
    /// GET /library/{artistId}/releases/{releaseFolderName}/coverart.{extension}
    /// </summary>
    [HttpGet("{artistId}/releases/{releaseFolderName}/coverart.{extension}")]
    public async Task<IActionResult> GetReleaseCoverArtWithExtension(
        string artistId,
        string releaseFolderName,
        string extension
    )
    {
        var (stream, contentType, fileName) = await _assetReader.GetReleaseCoverArtAsync(
            artistId,
            releaseFolderName
        );

        if (stream == null || contentType == null)
        {
            return NotFound(
                $"Cover art not found for release '{releaseFolderName}' by artist '{artistId}'"
            );
        }

        // Validate that the requested extension matches the actual file
        var actualExtension = System
            .IO.Path.GetExtension(fileName)
            ?.TrimStart('.')
            .ToLowerInvariant();
        if (actualExtension != extension.ToLowerInvariant())
        {
            return NotFound(
                $"Requested extension '{extension}' does not match actual file extension '{actualExtension}'"
            );
        }

        return File(stream, contentType, fileName);
    }

    /// <summary>
    /// Determines if the file extension should be included based on the request path
    /// </summary>
    private static bool ShouldIncludeFileExtension(string requestPath)
    {
        // If the path ends with a file extension, include it in the response
        var lastSegment = requestPath.Split('/').LastOrDefault() ?? "";
        return lastSegment.Contains('.');
    }
}
