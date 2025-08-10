using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Playlists.Events;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class AddTrackToPlaylistMutation
{
    public async Task<AddTrackToPlaylistResult> AddTrackToPlaylist(
        AddTrackToPlaylistInput input,
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
            return new AddTrackToPlaylistError("Not authenticated");
        }

        db.Events.Add(
            new SongAddedToPlaylist
            {
                PlaylistId = input.PlaylistId,
                PlaylistItemId = Guid.NewGuid().ToString(),
                ActorUserId = actorUserId,
                LocalArtistId = input.ArtistId,
                LocalReleaseFolderName = input.ReleaseFolderName,
                LocalTrackNumber = input.TrackNumber,
            }
        );
        await db.SaveChangesAsync();
        await eventProcessor.ProcessEvents();

        var playlistModel = await db.Playlists.FirstOrDefaultAsync(p => p.Id == input.PlaylistId);
        if (playlistModel == null)
        {
            return new AddTrackToPlaylistError("Playlist not found after update");
        }

        return new AddTrackToPlaylistSuccess(new Playlist(playlistModel));
    }
}

[GraphQLName("AddTrackToPlaylistInput")]
public record AddTrackToPlaylistInput(
    [property: ID] string PlaylistId,
    [property: ID] string ArtistId,
    string ReleaseFolderName,
    int TrackNumber
);

[UnionType]
public abstract record AddTrackToPlaylistResult;

public record AddTrackToPlaylistSuccess(Playlist Playlist) : AddTrackToPlaylistResult;

public record AddTrackToPlaylistError(string Message) : AddTrackToPlaylistResult;
