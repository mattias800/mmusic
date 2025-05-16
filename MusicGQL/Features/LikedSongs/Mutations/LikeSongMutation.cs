using MusicGQL.Features.LikedSongs.Commands;
using MusicGQL.Types;

namespace MusicGQL.Features.LikedSongs.Mutations;

[ExtendObjectType(typeof(Mutation))]
public record LikeSongMutation
{
    public async Task<LikeSongResult> LikeSong(
        [Service] LikeSongHandler likeSongHandler,
        LikeSongInput input
    )
    {
        return await likeSongHandler.Handle(new(Guid.NewGuid(), input.RecordingId)) switch
        {
            // TODO Correct user
            LikeSongHandler.Result.Success => new LikeSongResult.LikeSongSuccess(new User.User(0)),
            LikeSongHandler.Result.AlreadyLiked => new LikeSongResult.LikeSongAlreadyLiked(
                "Song already liked!"
            ),
            LikeSongHandler.Result.SongDoesNotExist => new LikeSongResult.LikeSongSongDoesNotExist(
                "Song does not exist in MusicBrainz!"
            ),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}

public record LikeSongInput(string RecordingId);

[UnionType("LikeSongResult")]
public abstract record LikeSongResult
{
    public record LikeSongSuccess(User.User Viewer) : LikeSongResult;

    public record LikeSongAlreadyLiked(string Message) : LikeSongResult;

    public record LikeSongSongDoesNotExist(string Message) : LikeSongResult;
}
