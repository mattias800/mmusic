using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models.Projections;
using MusicGQL.Features.LikedSongs;

namespace MusicGQL.Features.Users;

public record User([property: GraphQLIgnore] UserProjection Model)
{
    [ID]
    public string Id => Model.UserId.ToString();
    public string Username => Model.Username;

    public DateTime CreatedAt => Model.CreatedAt;
    public DateTime UpdatedAt => Model.UpdatedAt;

    public async Task<IEnumerable<LikedSong>> GetLikedSongs([Service] EventDbContext dbContext)
    {
        var likedSongsProjection = await dbContext.LikedSongsProjections.FirstOrDefaultAsync(p =>
            p.UserId == Model.UserId
        );
        if (likedSongsProjection == null || likedSongsProjection.LikedSongRecordingIds == null)
        {
            return Enumerable.Empty<LikedSong>();
        }

        return likedSongsProjection.LikedSongRecordingIds.Select(recordingId => new LikedSong(
            recordingId
        ));
    }
}
