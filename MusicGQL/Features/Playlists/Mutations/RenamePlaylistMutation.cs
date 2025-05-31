using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Playlists.Commands;
using MusicGQL.Features.Users;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class RenamePlaylistMutation
{
    [Authorize(Policy = "IsAuthenticatedUser")]
    public async Task<RenamePlaylistResult> RenamePlaylist(
        ClaimsPrincipal claimsPrincipal,
        RenamePlaylistHandler renamePlaylistHandler,
        EventDbContext dbContext,
        RenamePlaylistInput input
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

        await renamePlaylistHandler.Handle(
            new RenamePlaylistHandler.Command(
                userId,
                Guid.Parse(input.PlaylistId),
                input.NewPlaylistName
            )
        );

        return new RenamePlaylistSuccess(new User(user));
    }
}

public record RenamePlaylistInput(string PlaylistId, string NewPlaylistName);

[UnionType("RenamePlaylistResult")]
public abstract record RenamePlaylistResult;

public record RenamePlaylistSuccess(User Viewer) : RenamePlaylistResult;

public record RenamePlaylistNoWriteAccess() : RenamePlaylistResult;
