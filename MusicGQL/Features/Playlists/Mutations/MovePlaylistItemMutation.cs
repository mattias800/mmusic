using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Playlists.Db;
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
        var playlistItem = await db.Set<DbPlaylistItem>()
            .FirstOrDefaultAsync(i =>
                i.Id == input.PlaylistItemId && i.PlaylistId == input.PlaylistId
            );
        if (playlistItem == null)
        {
            return new MovePlaylistItemError("Playlist item not found");
        }

        db.Events.Add(
            new PlaylistItemMoved
            {
                PlaylistId = input.PlaylistId,
                PlaylistItemId = input.PlaylistItemId,
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
public record MovePlaylistItemInput(string PlaylistId, string PlaylistItemId, int NewIndex);

[UnionType]
public abstract record MovePlaylistItemResult;

public record MovePlaylistItemSuccess(Playlist Playlist) : MovePlaylistItemResult;

public record MovePlaylistItemError(string Message) : MovePlaylistItemResult;
