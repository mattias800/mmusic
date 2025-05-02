using MusicGQL.Db.Models;
using MusicGQL.Db.Models.Events;
using MusicGQL.Db.Models.Projections;

namespace MusicGQL.Aggregates.LikedSongs;

public static class LikedSongsReducer
{
    public static LikedSongsProjection Reduce(LikedSongsProjection projection, Event ev)
    {
        return ev switch
        {
            LikedSong e => projection.LikedSongReleaseIds.Any(releaseId => releaseId == e.ReleaseId)
                ? projection
                : projection with
                {
                    LikedSongReleaseIds = [..projection.LikedSongReleaseIds, e.ReleaseId]
                },
            UnlikedSong e => projection with
            {
                LikedSongReleaseIds =
                projection.LikedSongReleaseIds.Where(releaseId => releaseId != e.ReleaseId).ToList()
            },
            _ => projection
        };
    }
}