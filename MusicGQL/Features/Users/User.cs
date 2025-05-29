using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models.Projections;
using MusicGQL.Features.LikedSongs;
using MusicGQL.Features.Playlists;

namespace MusicGQL.Features.Users;

public record User([property: GraphQLIgnore] UserProjection Model)
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
        var playlistsProjection = await dbContext.PlaylistsProjections.FirstOrDefaultAsync(p =>
            p.UserId == Model.UserId
        );
        if (playlistsProjection == null)
        {
            return [];
        }

        return playlistsProjection.Playlists.Select(p => new Playlist(p));
    }
}
