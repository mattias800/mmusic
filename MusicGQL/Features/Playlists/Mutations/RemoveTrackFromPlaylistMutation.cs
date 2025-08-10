using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Playlists.Events;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class RemoveTrackFromPlaylistMutation
{
    public async Task<RemoveTrackFromPlaylistResult> RemoveTrackFromPlaylist(
        RemoveTrackFromPlaylistInput input,
        [Service] EventDbContext db,
        [Service] EventProcessor.EventProcessorWorker eventProcessor,
        [Service] IHttpContextAccessor httpContextAccessor
    )
    {
        var httpContext = httpContextAccessor.HttpContext;
        var userIdClaim = httpContext
            ?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var actorUserId))
        {
            return new RemoveTrackFromPlaylistError("Not authenticated");
        }
        var recordingId = $"{input.ArtistId}/{input.ReleaseFolderName}/{input.TrackNumber}";
        db.Events.Add(
            new SongRemovedFromPlaylist
            {
                PlaylistId = input.PlaylistId,
                RecordingId = recordingId,
                ActorUserId = actorUserId,
            }
        );
        await db.SaveChangesAsync();
        await eventProcessor.ProcessEvents();

        var playlistModel = await db.Playlists.FirstOrDefaultAsync(p => p.Id == input.PlaylistId);
        if (playlistModel == null)
        {
            return new RemoveTrackFromPlaylistError("Playlist not found after update");
        }

        return new RemoveTrackFromPlaylistSuccess(new Playlist(playlistModel));
    }
}

[GraphQLName("RemoveTrackFromPlaylistInput")]
public record RemoveTrackFromPlaylistInput(
    Guid PlaylistId,
    string ArtistId,
    string ReleaseFolderName,
    int TrackNumber
);

[UnionType]
public abstract record RemoveTrackFromPlaylistResult;

public record RemoveTrackFromPlaylistSuccess(Playlist Playlist) : RemoveTrackFromPlaylistResult;

public record RemoveTrackFromPlaylistError(string Message) : RemoveTrackFromPlaylistResult;
