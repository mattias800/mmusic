using Microsoft.EntityFrameworkCore;
using MusicGQL.Aggregates.LikedSongs;
using MusicGQL.Db;
using MusicGQL.Db.Models;
using MusicGQL.Db.Models.Projections;

namespace MusicGQL.Aggregates;

public class EventProcessor(EventDbContext dbContext, ILogger<EventProcessor> logger)
{
    public async Task ProcessEvents()
    {
        var checkpoint = await dbContext.EventCheckpoints.FindAsync("LikedSongs");
        var lastId = checkpoint?.LastProcessedEventId ?? 0;

        if (checkpoint == null)
        {
            checkpoint = new EventCheckpoint { Id = "LikedSongs" };
            dbContext.EventCheckpoints.Add(checkpoint);
        }

        var events = await dbContext
            .Events.Where(e => e.Id > lastId)
            .OrderBy(e => e.Id)
            .ToListAsync();

        logger.LogInformation("Processing {Count} unhandled events..", events.Count);

        var projection = await dbContext.LikedSongsProjections.FindAsync(1);

        if (projection == null)
        {
            projection = new LikedSongsProjection { Id = 1 };
            dbContext.LikedSongsProjections.Add(projection);
        }

        foreach (var e in events)
        {
            LikedSongsReducer.Reduce(projection, e);
            checkpoint.LastProcessedEventId = e.Id;
        }

        projection.LastUpdatedAt = DateTime.UtcNow;

        logger.LogInformation("Liked songs: {Songs}", projection.LikedSongRecordingIds);
        await dbContext.SaveChangesAsync();
    }
}
