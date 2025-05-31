using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Likes;
using MusicGQL.Features.Playlists;

namespace MusicGQL.Features.Users;

public record User([property: GraphQLIgnore] Db.DbUser Model)
{
    [ID]
    public string Id => Model.UserId.ToString();
    public string Username => Model.Username;

    public DateTime CreatedAt => Model.CreatedAt;
    public DateTime UpdatedAt => Model.UpdatedAt;

    public async Task<IEnumerable<LikedSong>> LikedSongs([Service] EventDbContext dbContext)
    {
        var likedSongsProjection = await dbContext.LikedSongsProjections.FirstOrDefaultAsync(p =>
            p.UserId == Model.UserId
        );
        if (likedSongsProjection == null)
        {
            return [];
        }

        return likedSongsProjection.LikedSongRecordingIds.Select(recordingId => new LikedSong(
            recordingId
        ));
    }

    public async Task<IEnumerable<Playlist>> Playlists([Service] EventDbContext dbContext)
    {
        var playlists = await dbContext
            .Playlists.Where(p => p.UserId == Model.UserId)
            .ToListAsync();

        return playlists.Select(p => new Playlist(p));
    }
}
