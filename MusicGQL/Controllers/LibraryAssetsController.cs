using Microsoft.AspNetCore.Mvc;
using MusicGQL.Features.ServerLibrary.Reader;

namespace MusicGQL.Controllers;

/// <summary>
/// Controller for serving music library assets (images, audio files)
/// </summary>
[ApiController]
[Route("library")]
public class LibraryAssetsController(ServerLibraryAssetReader assetReader) : ControllerBase
{
    /// <summary>
    /// Serves artist photos by type
    /// GET /library/{artistId}/photos/{photoType}/{photoIndex}
    /// </summary>
    [HttpGet("{artistId}/photos/{photoType}/{photoIndex:int}")]
    public async Task<IActionResult> GetArtistPhoto(
        string artistId,
        string photoType,
        int photoIndex
    )
    {
        // Validate photo type
        string[] validPhotoTypes = ["thumbs", "backgrounds", "banners", "logos"];
        if (!validPhotoTypes.Contains(photoType.ToLowerInvariant()))
        {
            return BadRequest(
                $"Invalid photo type '{photoType}'. Valid types are: {string.Join(", ", validPhotoTypes)}"
            );
        }

        var (stream, contentType, fileName) = await assetReader.GetArtistPhotoAsync(
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
        var (stream, contentType, fileName) = await assetReader.GetReleaseCoverArtAsync(
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
    /// Serves cover art for locally stored top track images (./toptrackNN.jpg in artist folder)
    /// GET /library/{artistId}/toptracks/{index}/coverart
    /// </summary>
    [HttpGet("{artistId}/toptracks/{index:int}/coverart")]
    public async Task<IActionResult> GetTopTrackCoverArt(string artistId, int index)
    {
        var (stream, contentType, fileName) = await assetReader.GetTopTrackCoverArtAsync(
            artistId,
            index
        );

        if (stream == null || contentType == null)
        {
            return NotFound(
                $"Top track cover art not found for artist '{artistId}' at index {index}"
            );
        }

        var shouldIncludeExtension = ShouldIncludeFileExtension(Request.Path);
        return File(stream, contentType, shouldIncludeExtension ? fileName : null);
    }

    /// <summary>
    /// Serves cover art for artist appearances (./appearance_*.jpg in artist folder)
    /// GET /library/{artistId}/appearances/{appearanceId}/coverart
    /// </summary>
    [HttpGet("{artistId}/appearances/{appearanceId}/coverart")]
    public async Task<IActionResult> GetAppearanceCoverArt(string artistId, string appearanceId)
    {
        var (stream, contentType, fileName) = await assetReader.GetAppearanceCoverArtAsync(
            artistId,
            appearanceId
        );

        if (stream == null || contentType == null)
        {
            return NotFound(
                $"Appearance cover art not found for artist '{artistId}' with appearance ID '{appearanceId}'"
            );
        }

        // Determine if we should include file extension in response
        var shouldIncludeExtension = ShouldIncludeFileExtension(Request.Path);

        return File(stream, contentType, shouldIncludeExtension ? fileName : null);
    }

    /// <summary>
    /// Serves similar artist thumbnail stored in the parent artist folder
    /// GET /library/{artistId}/similar/{musicBrainzArtistId}/thumb
    /// </summary>
    [HttpGet("{artistId}/similar/{musicBrainzArtistId}/thumb")]
    public async Task<IActionResult> GetSimilarArtistThumb(
        string artistId,
        string musicBrainzArtistId
    )
    {
        var (stream, contentType, fileName) = await assetReader.GetSimilarArtistThumbAsync(
            artistId,
            musicBrainzArtistId
        );
        if (stream == null || contentType == null)
        {
            return NotFound(
                $"Similar artist thumb not found for artist '{artistId}' and MBID '{musicBrainzArtistId}'"
            );
        }
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
        int trackNumber,
        [FromServices] Db.Postgres.EventDbContext dbContext,
        [FromServices] EventProcessor.EventProcessorWorker eventProcessor
    )
    {
        var (stream, contentType, fileName) = await assetReader.GetTrackAudioAsync(
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

        // Increment play-count in release.json and refresh the cache.
        // Emit a TrackPlayed event and process (projection will be updated by the event processor)
        var userIdClaim = HttpContext
            .User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?.Value;
        Guid? subjectUserId = null;
        if (Guid.TryParse(userIdClaim, out var parsed))
            subjectUserId = parsed;

        if (subjectUserId.HasValue)
        {
            var ev = new Features.PlayCounts.Events.TrackPlayed
            {
                ArtistId = artistId,
                ReleaseFolderName = releaseFolderName,
                TrackNumber = trackNumber,
                SubjectUserId = subjectUserId.Value,
            };
            dbContext.Events.Add(ev);
            await dbContext.SaveChangesAsync();
            await eventProcessor.ProcessEvents();
        }

        // Enable HTTP Range processing so the <audio> element can seek without restarting
        return File(stream, contentType, fileName, enableRangeProcessing: true);
    }

    /// <summary>
    /// Serves disc-aware track audio files
    /// GET /library/{artistId}/releases/{releaseFolderName}/discs/{discNumber}/tracks/{trackNumber}/audio
    /// </summary>
    [HttpGet(
        "{artistId}/releases/{releaseFolderName}/discs/{discNumber:int}/tracks/{trackNumber:int}/audio"
    )]
    public async Task<IActionResult> GetTrackAudioByDisc(
        string artistId,
        string releaseFolderName,
        int discNumber,
        int trackNumber,
        [FromServices] Db.Postgres.EventDbContext dbContext,
        [FromServices] EventProcessor.EventProcessorWorker eventProcessor
    )
    {
        var (stream, contentType, fileName) = await assetReader.GetTrackAudioAsync(
            artistId,
            releaseFolderName,
            discNumber,
            trackNumber
        );

        if (stream == null || contentType == null)
        {
            return NotFound(
                $"Audio file not found for disc {discNumber} track {trackNumber} in release '{releaseFolderName}' by artist '{artistId}'"
            );
        }

        // Increment play-count in release.json and refresh the cache.
        // Emit a TrackPlayed event and process (projection will be updated by the event processor)
        var userIdClaim = HttpContext
            .User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?.Value;
        Guid? subjectUserId = null;
        if (Guid.TryParse(userIdClaim, out var parsed))
            subjectUserId = parsed;

        if (subjectUserId.HasValue)
        {
            var ev = new Features.PlayCounts.Events.TrackPlayed
            {
                ArtistId = artistId,
                ReleaseFolderName = releaseFolderName,
                TrackNumber = trackNumber,
                SubjectUserId = subjectUserId.Value,
            };
            dbContext.Events.Add(ev);
            await dbContext.SaveChangesAsync();
            await eventProcessor.ProcessEvents();
        }

        // Enable HTTP Range processing so the <audio> element can seek without restarting
        return File(stream, contentType, fileName, enableRangeProcessing: true);
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
        string[] validPhotoTypes = ["thumbs", "backgrounds", "banners", "logos"];
        if (!validPhotoTypes.Contains(photoType.ToLowerInvariant()))
        {
            return BadRequest(
                $"Invalid photo type '{photoType}'. Valid types are: {string.Join(", ", validPhotoTypes)}"
            );
        }

        var (stream, contentType, fileName) = await assetReader.GetArtistPhotoAsync(
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
        var (stream, contentType, fileName) = await assetReader.GetReleaseCoverArtAsync(
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
    /// Alternative endpoint with file extension for appearance cover art
    /// GET /library/{artistId}/appearances/{appearanceId}/coverart.{extension}
    /// </summary>
    [HttpGet("{artistId}/appearances/{appearanceId}/coverart.{extension}")]
    public async Task<IActionResult> GetAppearanceCoverArtWithExtension(
        string artistId,
        string appearanceId,
        string extension
    )
    {
        var (stream, contentType, fileName) = await assetReader.GetAppearanceCoverArtAsync(
            artistId,
            appearanceId
        );

        if (stream == null || contentType == null)
        {
            return NotFound(
                $"Appearance cover art not found for artist '{artistId}' with appearance ID '{appearanceId}'"
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
