using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Playlists.Commands;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class CreatePlaylistMutation
{
    [Authorize(Policy = "IsAuthenticatedUser")]
    public async Task<CreatePlaylistResult> CreatePlaylist(
        ClaimsPrincipal claimsPrincipal,
        CreatePlaylistHandler createPlaylistHandler,
        EventDbContext dbContext,
        CreatePlaylistInput input
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

        // Authorization: require CreatePlaylists role or Admin
        if ((user.Roles & (Users.Roles.UserRoles.CreatePlaylists | Users.Roles.UserRoles.Admin)) == 0)
        {
            return new CreatePlaylistNoWriteAccess();
        }

        await createPlaylistHandler.Handle(new CreatePlaylistHandler.Command(userId));

        // Fetch the newly created playlist for this user (latest by CreatedAt)
        var playlist = await dbContext
            .Playlists.Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

        return playlist == null
            ? new CreatePlaylistError("Playlist was created but not found")
            : new CreatePlaylistSuccess(new Playlist(playlist));
    }
}

[GraphQLName("CreatePlaylistInput")]
public record CreatePlaylistInput(string? Name, string? Description);

[UnionType("CreatePlaylistResult")]
public abstract record CreatePlaylistResult;

public record CreatePlaylistSuccess(Playlist Playlist) : CreatePlaylistResult;

public record CreatePlaylistNoWriteAccess() : CreatePlaylistResult;

public record CreatePlaylistError(string Message) : CreatePlaylistResult;
