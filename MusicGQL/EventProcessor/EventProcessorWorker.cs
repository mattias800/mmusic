using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models;
using MusicGQL.Features.Likes.Events;
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

        foreach (var ev in events)
        {
            await releaseGroupsAddedToServerLibraryProcessor.ProcessEvent(ev, dbContext);
            await artistsAddedToServerLibraryProcessor.ProcessEvent(ev, dbContext);
            await likedSongsEventProcessor.ProcessEvent(ev, dbContext);
            await userEventProcessor.ProcessEvent(ev, dbContext);
            await playlistsEventProcessor.ProcessEvent(ev, dbContext);
        }

        if (events.Count > 0)
        {
            checkpoint.LastProcessedEventId = events.Last().Id;
        }

        await dbContext.SaveChangesAsync();
    }
}
