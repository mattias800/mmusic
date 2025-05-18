using MusicGQL.Db;
using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models;
using MusicGQL.Db.Postgres.Models.Events;
using MusicGQL.Db.Postgres.Models.Projections;

namespace MusicGQL.Features.LikedSongs.Aggregate;

public class LikedSongsEventProcessor : EventProcessor.EventProcessor<LikedSongsProjection>
{
    private LikedSongsProjection? _projection;

    public override async Task PrepareProcessing(EventDbContext dbContext)
    {
        _projection = await dbContext.LikedSongsProjections.FindAsync(1);

        if (_projection == null)
        {
            _projection = new LikedSongsProjection { Id = 1 };
            dbContext.LikedSongsProjections.Add(_projection);
        }
    }

    public override LikedSongsProjection? GetAggregate()
    {
        return _projection;
    }

    protected override void ProcessEvent(
        Event ev,
        EventDbContext dbContext,
        LikedSongsProjection aggregate
    )
    {
        switch (ev)
        {
            case Db.Postgres.Models.Events.LikedSong e:
                if (!aggregate.LikedSongRecordingIds.Contains(e.RecordingId))
                {
                    aggregate.LikedSongRecordingIds.Add(e.RecordingId);
                    aggregate.LastUpdatedAt = DateTime.UtcNow;
                }

                break;

            case UnlikedSong e:
                aggregate.LikedSongRecordingIds.RemoveAll(id => id == e.RecordingId);
                break;
        }
    }
}
