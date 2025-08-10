using MusicGQL.Db.Postgres;
using MusicGQL.Features.Playlists.Events;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Mutations;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class AddTrackToPlaylistMutation
{
    public async Task<AddTrackToPlaylistResult> AddTrackToPlaylist(
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
            return new AddTrackToPlaylistError("Not authenticated");
        }
        var recordingId = $"{artistId}/{releaseFolderName}/{trackNumber}";
        db.Events.Add(
            new SongAddedToPlaylist
            {
                PlaylistId = playlistId,
                RecordingId = recordingId,
                ActorUserId = actorUserId,
            }
        );
        await db.SaveChangesAsync();
        await eventProcessor.ProcessEvents();
        return new AddTrackToPlaylistSuccess(true);
    }
}

[UnionType]
public abstract record AddTrackToPlaylistResult;
public record AddTrackToPlaylistSuccess(bool Success) : AddTrackToPlaylistResult;
public record AddTrackToPlaylistError(string Message) : AddTrackToPlaylistResult;


