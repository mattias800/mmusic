using MusicGQL.Db;
using MusicGQL.Features.LikedSongs;

namespace MusicGQL.Features.User;

public record User(int Id)
{
    public async Task<IEnumerable<LikedSong>> LikedSongs(EventDbContext dbContext)
    {
        var likedSongs = await dbContext.LikedSongsProjections.FindAsync(1);
        if (likedSongs == null)
        {
            return [];
        }

        return likedSongs.LikedSongRecordingIds.Select(recordingId => new LikedSong(recordingId));
    }
}
