using Microsoft.EntityFrameworkCore;
using MusicGQL.Db;
using MusicGQL.Db.Models;
using MusicGQL.Features.LikedSongs.Aggregate;
using MusicGQL.Features.ServerLibrary.Artist.Aggregate;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Aggregate;

namespace MusicGQL.EventProcessor;

public class EventProcessorWorker(
    EventDbContext dbContext,
    ILogger<EventProcessorWorker> logger,
    ReleaseGroupsAddedToServerLibraryProcessor releaseGroupsAddedToServerLibraryProcessor,
    ArtistsAddedToServerLibraryProcessor artistsAddedToServerLibraryProcessor,
    LikedSongsEventProcessor likedSongsEventProcessor
)
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

        await releaseGroupsAddedToServerLibraryProcessor.PrepareProcessing(dbContext);
        await artistsAddedToServerLibraryProcessor.PrepareProcessing(dbContext);
        await likedSongsEventProcessor.PrepareProcessing(dbContext);

        foreach (var ev in events)
        {
            releaseGroupsAddedToServerLibraryProcessor.ProcessEvent(ev, dbContext);
            artistsAddedToServerLibraryProcessor.ProcessEvent(ev, dbContext);
            likedSongsEventProcessor.ProcessEvent(ev, dbContext);
        }

        await dbContext.SaveChangesAsync();
    }
}
