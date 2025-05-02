using MusicGQL.Features.LikedSongs;

namespace MusicGQL.Features.User;

public record User(int Id)
{
    public IEnumerable<LikedSong> LikedSongs = [];
}