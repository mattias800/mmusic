using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Playlists.Commands;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class DeletePlaylistMutation
{
    [Authorize(Policy = "IsAuthenticatedUser")]
    public async Task<DeletePlaylistResult> DeletePlaylist(
        ClaimsPrincipal claimsPrincipal,
        DeletePlaylistHandler renamePlaylistHandler,
        EventDbContext dbContext,
        DeletePlaylistInput input
    )
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        var userId = Guid.Parse(userIdClaim!.Value);

        var user = await dbContext.Users.FirstOrDefaultAsync(up => up.UserId == userId);
        if (user is null)
        {
            throw new GraphQLException(
                ErrorBuilder
                    .New()
                    .SetMessage("Authenticated user not found after authorization policy success.")
                    .SetCode("INTERNAL_SERVER_ERROR")
                    .Build()
            );
        }

        return await renamePlaylistHandler.Handle(
            new DeletePlaylistHandler.Command(userId, input.PlaylistId)
        ) switch
        {
            DeletePlaylistHandler.Result.NotAllowed => new DeletePlaylistNoWriteAccess(
                "You do not have write access to this playlist."
            ),
            DeletePlaylistHandler.Result.Success => new DeletePlaylistSuccess(
                Guid.Parse(input.PlaylistId)
            ),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}

public record DeletePlaylistInput([property: ID] string PlaylistId);

[UnionType("DeletePlaylistResult")]
public abstract record DeletePlaylistResult;

public record DeletePlaylistSuccess([ID] Guid DeletedPlaylistId) : DeletePlaylistResult;

public record DeletePlaylistNoWriteAccess(string Message) : DeletePlaylistResult;
