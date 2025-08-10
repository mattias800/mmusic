using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Playlists.Events;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class MovePlaylistItemMutation
{
    public async Task<MovePlaylistItemResult> MovePlaylistItem(
        MovePlaylistItemInput input,
        [Service] EventDbContext db,
        [Service] EventProcessor.EventProcessorWorker eventProcessor,
        [Service] IHttpContextAccessor httpContextAccessor
    )
    {
        var httpContext = httpContextAccessor.HttpContext;
        var userIdClaim = httpContext
            ?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return new MovePlaylistItemError("Not authenticated");
        }
        var recordingId = $"{input.ArtistId}/{input.ReleaseFolderName}/{input.TrackNumber}";
        db.Events.Add(
            new PlaylistItemMoved
            {
                PlaylistId = input.PlaylistId,
                RecordingId = recordingId,
                NewIndex = input.NewIndex,
                ActorUserId = userId,
            }
        );
        await db.SaveChangesAsync();
        await eventProcessor.ProcessEvents();

        var playlistModel = await db.Playlists.FirstOrDefaultAsync(p => p.Id == input.PlaylistId);
        if (playlistModel == null)
        {
            return new MovePlaylistItemError("Playlist not found after update");
        }

        return new MovePlaylistItemSuccess(new Playlist(playlistModel));
    }
}

[GraphQLName("MovePlaylistItemInput")]
public record MovePlaylistItemInput(
    Guid PlaylistId,
    string ArtistId,
    string ReleaseFolderName,
    int TrackNumber,
    int NewIndex
);

[UnionType]
public abstract record MovePlaylistItemResult;

public record MovePlaylistItemSuccess(Playlist Playlist) : MovePlaylistItemResult;

public record MovePlaylistItemError(string Message) : MovePlaylistItemResult;
