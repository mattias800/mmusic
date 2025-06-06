using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Playlists.Commands;
using MusicGQL.Features.Users;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class CreatePlaylistMutation
{
    [Authorize(Policy = "IsAuthenticatedUser")]
    public async Task<CreatePlaylistResult> CreatePlaylist(
        ClaimsPrincipal claimsPrincipal,
        CreatePlaylistHandler createPlaylistHandler,
        EventDbContext dbContext
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

        await createPlaylistHandler.Handle(new CreatePlaylistHandler.Command(userId));

        return new CreatePlaylistSuccess(new User(user));
    }
}

[UnionType("CreatePlaylistResult")]
public abstract record CreatePlaylistResult;

public record CreatePlaylistSuccess(User Viewer) : CreatePlaylistResult;

public record CreatePlaylistNoWriteAccess() : CreatePlaylistResult;
