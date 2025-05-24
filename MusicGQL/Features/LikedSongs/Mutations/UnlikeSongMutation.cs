using System;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate.Types;
using Microsoft.AspNetCore.Http;
using MusicGQL.Features.LikedSongs.Commands;
using MusicGQL.Types;

namespace MusicGQL.Features.LikedSongs.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class UnlikeSongMutation
{
    public async Task<UnlikedSongPayload> UnlikeSong(
        UnlikedSongInput input,
        [Service] UnlikeSongHandler unlikeSongHandler,
        [Service] IHttpContextAccessor httpContextAccessor
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
            UnlikeSongHandler.Result.Success => new UnlikedSongPayload(true),
            UnlikeSongHandler.Result.AlreadyNotLiked => new UnlikedSongPayload(true),
            _ => throw new ArgumentOutOfRangeException(
                nameof(handlerResult),
                "Unhandled result from UnlikeSongHandler"
            ),
        };
    }
}

public record UnlikedSongInput(string RecordingId);

public record UnlikedSongPayload(bool Success);
