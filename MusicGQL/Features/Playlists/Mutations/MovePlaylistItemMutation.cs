using MusicGQL.Db.Postgres;
using MusicGQL.Features.Playlists.Events;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Mutations;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class MovePlaylistItemMutation
{
    public async Task<MovePlaylistItemResult> MovePlaylistItem(
        [Service] EventDbContext db,
        [Service] EventProcessor.EventProcessorWorker eventProcessor,
        [Service] IHttpContextAccessor httpContextAccessor,
        Guid playlistId,
        string artistId,
        string releaseFolderName,
        int trackNumber,
        int newIndex,
        Guid actorUserId
    )
    {
        var httpContext = httpContextAccessor.HttpContext;
        var userIdClaim = httpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return new MovePlaylistItemError("Not authenticated");
        }
        var recordingId = $"{artistId}/{releaseFolderName}/{trackNumber}";
        db.Events.Add(
            new PlaylistItemMoved
            {
                PlaylistId = playlistId,
                RecordingId = recordingId,
                NewIndex = newIndex,
                ActorUserId = userId,
            }
        );
        await db.SaveChangesAsync();
        await eventProcessor.ProcessEvents();
        return new MovePlaylistItemSuccess(true);
    }
}

[UnionType]
public abstract record MovePlaylistItemResult;
public record MovePlaylistItemSuccess(bool Success) : MovePlaylistItemResult;
public record MovePlaylistItemError(string Message) : MovePlaylistItemResult;


