using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Likes.Commands;
using MusicGQL.Features.Users;
using MusicGQL.Types;

// For ClaimTypes
// For FirstOrDefaultAsync
// For EventDbContext (to fetch UserProjection for Viewer)
// For User type

// For Mutation base

namespace MusicGQL.Features.Likes.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class LikeSongMutation // Changed to class as it now has dependencies
{
    public async Task<LikeSongResult> LikeSong(
        LikeSongInput input,
        [Service] LikeSongHandler likeSongHandler,
        [Service] IHttpContextAccessor httpContextAccessor, // Inject IHttpContextAccessor
        [Service] EventDbContext dbContext // Inject EventDbContext to fetch viewer details
    )
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );
        if (!Guid.TryParse(userIdString, out var userId))
        {
            // User not authenticated or UserId claim is missing/invalid
            // Return an appropriate error. For now, throwing, but a GraphQL error object is better.
            // This could also be a specific LikeSongResult type like LikeSongResult.NotAuthenticated
            throw new UnauthorizedAccessException("User not authenticated.");
        }

        var handlerResult = await likeSongHandler.Handle(
            new LikeSongHandler.Command(userId, input.RecordingId)
        );

        switch (handlerResult)
        {
            case LikeSongHandler.Result.Success:
                var userProjection = await dbContext.UserProjections.FirstOrDefaultAsync(u =>
                    u.UserId == userId
                );
                if (userProjection == null)
                {
                    // This case should ideally not happen if user is authenticated
                    throw new Exception("Authenticated user projection not found.");
                }
                return new LikeSongResult.LikeSongSuccess(new User(userProjection));
            case LikeSongHandler.Result.AlreadyLiked:
                return new LikeSongResult.LikeSongAlreadyLiked("Song already liked!");
            case LikeSongHandler.Result.SongDoesNotExist:
                return new LikeSongResult.LikeSongSongDoesNotExist(
                    "Song does not exist in MusicBrainz!"
                );
            default:
                // Log error: Unhandled handler result
                throw new ArgumentOutOfRangeException(
                    nameof(handlerResult),
                    "Unhandled result from LikeSongHandler"
                );
        }
    }
}

public record LikeSongInput(string RecordingId);

[UnionType("LikeSongResult")]
public abstract record LikeSongResult
{
    public record LikeSongSuccess(User Viewer) : LikeSongResult;

    public record LikeSongAlreadyLiked(string Message) : LikeSongResult;

    public record LikeSongSongDoesNotExist(string Message) : LikeSongResult;
    // Consider adding: public record NotAuthenticated(string Message) : LikeSongResult;
}
