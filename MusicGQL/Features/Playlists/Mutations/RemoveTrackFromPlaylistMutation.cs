using MusicGQL.Db.Postgres;
using MusicGQL.Features.Playlists.Events;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Mutations;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class RemoveTrackFromPlaylistMutation
{
    public async Task<RemoveTrackFromPlaylistResult> RemoveTrackFromPlaylist(
        [Service] EventDbContext db,
        [Service] EventProcessor.EventProcessorWorker eventProcessor,
        [Service] IHttpContextAccessor httpContextAccessor,
        Guid playlistId,
        string artistId,
        string releaseFolderName,
        int trackNumber
    )
    {
        var httpContext = httpContextAccessor.HttpContext;
        var userIdClaim = httpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var actorUserId))
        {
            return new RemoveTrackFromPlaylistError("Not authenticated");
        }
        var recordingId = $"{artistId}/{releaseFolderName}/{trackNumber}";
        db.Events.Add(
            new SongRemovedFromPlaylist
            {
                PlaylistId = playlistId,
                RecordingId = recordingId,
                ActorUserId = actorUserId,
            }
        );
        await db.SaveChangesAsync();
        await eventProcessor.ProcessEvents();
        return new RemoveTrackFromPlaylistSuccess(true);
    }
}

[UnionType]
public abstract record RemoveTrackFromPlaylistResult;
public record RemoveTrackFromPlaylistSuccess(bool Success) : RemoveTrackFromPlaylistResult;
public record RemoveTrackFromPlaylistError(string Message) : RemoveTrackFromPlaylistResult;


