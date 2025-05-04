using MusicGQL.Db.Models;
using MusicGQL.Db.Models.Events;
using MusicGQL.Db.Models.Projections;

namespace MusicGQL.Aggregates.LikedSongs;

public static class LikedSongsReducer
{
    public static void Reduce(LikedSongsProjection projection, Event ev)
    {
        switch (ev)
        {
            case LikedSong e:
                if (!projection.LikedSongRecordingIds.Contains(e.RecordingId))
                {
                    projection.LikedSongRecordingIds.Add(e.RecordingId);
                }

                break;

            case UnlikedSong e:
                projection.LikedSongRecordingIds.RemoveAll(id => id == e.RecordingId);
                break;
        }
    }
}
