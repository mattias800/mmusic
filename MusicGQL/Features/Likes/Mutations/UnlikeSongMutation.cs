using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Likes.Commands;
using MusicGQL.Types;

namespace MusicGQL.Features.Likes.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class UnlikeSongMutation
{
    public async Task<UnlikeSongResult> UnlikeSong(
        UnlikedSongInput input,
        [Service] UnlikeSongHandler unlikeSongHandler,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] EventDbContext dbContext
    )
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );
        if (!Guid.TryParse(userIdString, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated.");
        }

        var handlerResult = await unlikeSongHandler.Handle(
            new UnlikeSongHandler.Command(userId, input.RecordingId)
        );

        return handlerResult switch
        {
            UnlikeSongHandler.Result.Success => await dbContext.Users.FirstOrDefaultAsync(u =>
                u.UserId == userId
            )
                is { } userProjection
                ? new UnlikeSongSuccess(new Users.User(userProjection))
                : throw new Exception("Authenticated user projection not found."),
            UnlikeSongHandler.Result.AlreadyNotLiked => new UnlikeSongAlreadyNotLiked(
                "Song was not liked"
            ),
            _ => throw new ArgumentOutOfRangeException(
                nameof(handlerResult),
                "Unhandled result from UnlikeSongHandler"
            ),
        };
    }
}

public record UnlikedSongInput(string RecordingId);

[UnionType("UnlikeSongResult")]
public abstract record UnlikeSongResult;

public record UnlikeSongSuccess(Users.User Viewer) : UnlikeSongResult;

public record UnlikeSongAlreadyNotLiked(string Message) : UnlikeSongResult;
