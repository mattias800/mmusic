using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Playlists.Commands;
using MusicGQL.Features.Users;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class CreatePlaylistMutation
{
    public async Task<CreatePlaylistResult> CreatePlaylist(
        IHttpContextAccessor httpContextAccessor,
        CreatePlaylistHandler createPlaylistHandler,
        EventDbContext dbContext
    )
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return new CreatePlaylistNotAuthenticated("HttpContext is null.");
        }

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is null)
        {
            return new CreatePlaylistNotAuthenticated("User is not authenticated.");
        }

        var userId = Guid.Parse(userIdClaim.Value);

        var userProjection = await dbContext.UserProjections.FirstOrDefaultAsync(up =>
            up.UserId == userId
        );

        if (userProjection is null)
        {
            return new CreatePlaylistNotAuthenticated("User is not authenticated.");
        }

        await createPlaylistHandler.Handle(new CreatePlaylistHandler.Command(userId));

        return new CreatePlaylistSuccess(new User(userProjection));
    }
}

[UnionType("CreatePlaylistResult")]
public abstract record CreatePlaylistResult;

public record CreatePlaylistSuccess(User Viewer) : CreatePlaylistResult;

public record CreatePlaylistNotAuthenticated(string Message) : CreatePlaylistResult;
