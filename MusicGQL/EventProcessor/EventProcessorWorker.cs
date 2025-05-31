using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models;
using MusicGQL.Features.LikedSongs.Aggregate;
using MusicGQL.Features.Playlists.Aggregate;
using MusicGQL.Features.ServerLibrary.Artist.Aggregate;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Aggregate;
using MusicGQL.Features.Users.Aggregate;

namespace MusicGQL.EventProcessor;

public class EventProcessorWorker(
    EventDbContext dbContext,
    ILogger<EventProcessorWorker> logger,
    ReleaseGroupsAddedToServerLibraryProcessor releaseGroupsAddedToServerLibraryProcessor,
    ArtistsAddedToServerLibraryProcessor artistsAddedToServerLibraryProcessor,
    LikedSongsEventProcessor likedSongsEventProcessor,
    UserEventProcessor userEventProcessor,
    PlaylistsEventProcessor playlistsEventProcessor
)
{
    public async Task ProcessEvents()
    {
        var checkpoint = await dbContext.EventCheckpoints.FindAsync("DefaultEventProcessor");
        var lastId = checkpoint?.LastProcessedEventId ?? 0;

        if (checkpoint == null)
        {
            checkpoint = new EventCheckpoint { Id = "DefaultEventProcessor" };
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
        await userEventProcessor.PrepareProcessing(dbContext);
        await playlistsEventProcessor.PrepareProcessing(dbContext);

        foreach (var ev in events)
        {
            releaseGroupsAddedToServerLibraryProcessor.ProcessEvent(ev, dbContext);
            artistsAddedToServerLibraryProcessor.ProcessEvent(ev, dbContext);
            likedSongsEventProcessor.ProcessEvent(ev, dbContext);
            userEventProcessor.ProcessEvent(ev, dbContext);
            await playlistsEventProcessor.ProcessEvent(ev, dbContext);
        }

        if (events.Count > 0)
        {
            checkpoint.LastProcessedEventId = events.Last().Id;
        }

        await dbContext.SaveChangesAsync();
    }
}
