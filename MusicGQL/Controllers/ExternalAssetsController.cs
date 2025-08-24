using Microsoft.AspNetCore.Mvc;
using MusicGQL.Features.Assets;

namespace MusicGQL.Controllers;

[ApiController]
[Route("assets")]
public class ExternalAssetsController(ExternalAssetStorage storage) : ControllerBase
{
    /// <summary>
    /// Serves locally cached cover art for imported playlist tracks
    /// GET /assets/coverart/{playlistId}/{trackId}
    /// </summary>
    [HttpGet("coverart/{playlistId:guid}/{trackId}")]
    public IActionResult GetImportedCoverArt(Guid playlistId, string trackId)
    {
        var (stream, contentType, fileName) = storage.TryOpenCoverImage(playlistId, trackId);
        if (stream == null || contentType == null)
        {
            return NotFound();
        }
        return File(stream, contentType, fileName);
    }
}
