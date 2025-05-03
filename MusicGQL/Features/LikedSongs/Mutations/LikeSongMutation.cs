using MusicGQL.Features.LikedSongs.Commands;
using MusicGQL.Types;

namespace MusicGQL.Features.LikedSongs.Mutations;

[ExtendObjectType(typeof(Mutation))]
public record LikeSongMutation
{
    public async Task<LikedSongResult> LikeSong([Service] LikeSongHandler likeSongHandler,
        LikedSongInput input)
    {
        return await likeSongHandler.Handle(new(Guid.NewGuid(), input.RecordingId)) switch
        {
            // TODO Correct user
            LikeSongHandler.Result.Success => new LikedSongSuccess(new User.User(0)),
            LikeSongHandler.Result.AlreadyLiked => new LikedSongAlreadyLiked("Song already liked!"),
            LikeSongHandler.Result.SongDoesNotExist => new LikedSongSongDoesNotExist(
                "Song does not exist in MusicBrainz!"),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public record LikedSongInput(string RecordingId);

[UnionType("LikedSongResult")]
public abstract record LikedSongResult
{
};

public record LikedSongSuccess(User.User Viewer) : LikedSongResult;

public record LikedSongAlreadyLiked(string Message) : LikedSongResult;

public record LikedSongSongDoesNotExist(string Message) : LikedSongResult;