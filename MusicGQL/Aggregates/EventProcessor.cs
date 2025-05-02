using Microsoft.EntityFrameworkCore;
using MusicGQL.Aggregates.LikedSongs;
using MusicGQL.Db;
using MusicGQL.Db.Models.Projections;

namespace MusicGQL.Aggregates;

public class EventProcessor(EventDbContext dbContext)
{
    public async Task ProcessEvents()
    {
        var checkpoint = await dbContext.EventCheckpoints.FindAsync("LikedSongs");
        var lastId = checkpoint?.LastProcessedEventId ?? 0;

        checkpoint ??= new EventCheckpoint { Id = "LikedSongs" };

        var events = await dbContext.Events
            .Where(e => e.Id > lastId)
            .OrderBy(e => e.Id)
            .ToListAsync();

        var projection = await dbContext.LikedSongsProjections.FindAsync(1) ?? new LikedSongsProjection();

        foreach (var e in events)
        {
            projection = LikedSongsReducer.Reduce(projection, e);
            checkpoint.LastProcessedEventId = e.Id;
        }

        projection.LastUpdatedAt = DateTime.UtcNow;

        dbContext.Update(projection);
        dbContext.Update(checkpoint);
        await dbContext.SaveChangesAsync();
    }
}