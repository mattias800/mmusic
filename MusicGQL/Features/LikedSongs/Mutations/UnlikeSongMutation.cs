using MusicGQL.Features.LikedSongs.Commands;
using MusicGQL.Types;

namespace MusicGQL.Features.LikedSongs.Mutations;

[ExtendObjectType(typeof(Mutation))]
public record UnlikeSongMutation
{
    public async Task<UnlikedSongPayload> UnlikeSong(
        [Service] UnlikeSongHandler unlikeSongHandler,
        UnlikedSongInput input
    )
    {
        return await unlikeSongHandler.Handle(new(Guid.NewGuid(), input.RecordingId)) switch
        {
            UnlikeSongHandler.Result.Success => new(true),
            UnlikeSongHandler.Result.AlreadyNotLiked => new(true),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}

public record UnlikedSongInput(string RecordingId);

public record UnlikedSongPayload(bool Success);
