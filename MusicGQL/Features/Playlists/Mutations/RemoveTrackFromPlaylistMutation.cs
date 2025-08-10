using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Playlists.Events;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class RemoveItemFromPlaylistMutation
{
    public async Task<RemoveItemFromPlaylistResult> RemoveItemFromPlaylist(
        RemoveItemFromPlaylistInput input,
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
            return new RemoveItemFromPlaylistError("Not authenticated");
        }
        var playlistItem = await db.Set<Db.DbPlaylistItem>()
            .FirstOrDefaultAsync(i =>
                i.Id == input.PlaylistItemId && i.PlaylistId == input.PlaylistId
            );
        if (playlistItem == null)
        {
            return new RemoveItemFromPlaylistError("Playlist item not found");
        }

        db.Events.Add(
            new SongRemovedFromPlaylist
            {
                PlaylistId = input.PlaylistId,
                PlaylistItemId = input.PlaylistItemId,
                ActorUserId = actorUserId,
            }
        );
        await db.SaveChangesAsync();
        await eventProcessor.ProcessEvents();

        var playlistModel = await db.Playlists.FirstOrDefaultAsync(p => p.Id == input.PlaylistId);
        if (playlistModel == null)
        {
            return new RemoveItemFromPlaylistError("Playlist not found after update");
        }

        return new RemoveItemFromPlaylistSuccess(new Playlist(playlistModel));
    }
}

[GraphQLName("RemoveItemFromPlaylistInput")]
public record RemoveItemFromPlaylistInput(
    [property: ID] string PlaylistId,
    [property: ID] string PlaylistItemId
);

[UnionType]
public abstract record RemoveItemFromPlaylistResult;

public record RemoveItemFromPlaylistSuccess(Playlist Playlist) : RemoveItemFromPlaylistResult;

public record RemoveItemFromPlaylistError(string Message) : RemoveItemFromPlaylistResult;
