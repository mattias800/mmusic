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
            LikedSong e => projection.LikedSongRecordingIds.Any(releaseId => releaseId == e.RecordingId)
                ? projection
                : projection with
                {
                    LikedSongRecordingIds = [..projection.LikedSongRecordingIds, e.RecordingId]
                },
            UnlikedSong e => projection with
            {
                LikedSongRecordingIds =
                projection.LikedSongRecordingIds.Where(releaseId => releaseId != e.RecordingId).ToList()
            },
            _ => projection
        };
    }
}