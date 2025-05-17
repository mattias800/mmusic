using AutoMapper;
using HotChocolate.Subscriptions;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace MusicGQL.Features.ServerLibrary.Artist.Sagas;

public class AddArtistToServerLibrarySaga(
    IBus bus,
    ITopicEventSender sender,
    EventDbContext dbContext,
    ILogger<AddArtistToServerLibrarySaga> logger,
    IMapper mapper
)
    : Saga<AddArtistToServerLibrarySagaData>,
        IAmInitiatedBy<AddArtistToServerLibrarySagaEvents.StartAddArtist>,
        IHandleMessages<AddArtistToServerLibrarySagaEvents.FoundArtistInMusicBrainz>,
        IHandleMessages<AddArtistToServerLibrarySagaEvents.DidNotFindArtistInMusicBrainz>
{
    protected override void CorrelateMessages(
        ICorrelationConfig<AddArtistToServerLibrarySagaData> config
    )
    {
        config.Correlate<AddArtistToServerLibrarySagaEvents.StartAddArtist>(
            m => m.ArtistMbId,
            s => s.ArtistMbId
        );
        config.Correlate<AddArtistToServerLibrarySagaEvents.FindArtistInMusicBrainz>(
            m => m.ArtistMbId,
            s => s.ArtistMbId
        );
        config.Correlate<AddArtistToServerLibrarySagaEvents.FoundArtistInMusicBrainz>(
            m => m.ArtistMbId,
            s => s.ArtistMbId
        );
        config.Correlate<AddArtistToServerLibrarySagaEvents.DidNotFindArtistInMusicBrainz>(
            m => m.ArtistMbId,
            s => s.ArtistMbId
        );
    }

    public async Task Handle(AddArtistToServerLibrarySagaEvents.StartAddArtist message)
    {
        if (!IsNew)
        {
            return;
        }

        logger.LogInformation("Starting AddArtistToServerLibrarySaga");

        var existingArtist = await dbContext.Artists.FirstOrDefaultAsync(a =>
            a.Id == message.ArtistMbId
        );

        if (existingArtist is not null)
        {
            logger.LogInformation("Artist is already in the library");

            MarkAsComplete();
            return;
        }

        Data.StatusDescription = "Looking up release";

        // await sender.SendAsync(nameof(DownloadSubscription.DownloadStarted), Data);

        await bus.Send(
            new AddArtistToServerLibrarySagaEvents.FindArtistInMusicBrainz(new(message.ArtistMbId))
        );
    }

    public async Task Handle(AddArtistToServerLibrarySagaEvents.FoundArtistInMusicBrainz message)
    {
        logger.LogInformation("Saving artist to library database");

        var dbArtist = mapper.Map<Db.Models.ServerLibrary.Artist>(message.Artist);

        // Handle Area
        if (dbArtist.Area != null)
        {
            var existingArea = await dbContext.Areas.FindAsync(dbArtist.Area.Id);
            if (existingArea != null)
            {
                dbContext.Entry(existingArea).CurrentValues.SetValues(dbArtist.Area);
                dbArtist.Area = existingArea;
            }
            else
            {
                dbContext.Areas.Add(dbArtist.Area);
            }
        }

        // Handle BeginArea
        if (dbArtist.BeginArea != null)
        {
            // If BeginArea is the same as Area, reuse the tracked entity
            if (dbArtist.Area != null && dbArtist.BeginArea.Id == dbArtist.Area.Id)
            {
                dbArtist.BeginArea = dbArtist.Area;
            }
            else
            {
                var existingBeginArea = await dbContext.Areas.FindAsync(dbArtist.BeginArea.Id);
                if (existingBeginArea != null)
                {
                    dbContext.Entry(existingBeginArea).CurrentValues.SetValues(dbArtist.BeginArea);
                    dbArtist.BeginArea = existingBeginArea;
                }
                else
                {
                    dbContext.Areas.Add(dbArtist.BeginArea);
                }
            }
        }

        // Handle EndArea
        if (dbArtist.EndArea != null)
        {
            // If EndArea is the same as Area, reuse the tracked entity
            if (dbArtist.Area != null && dbArtist.EndArea.Id == dbArtist.Area.Id)
            {
                dbArtist.EndArea = dbArtist.Area;
            }
            // If EndArea is the same as BeginArea, reuse the tracked entity
            else if (dbArtist.BeginArea != null && dbArtist.EndArea.Id == dbArtist.BeginArea.Id)
            {
                dbArtist.EndArea = dbArtist.BeginArea;
            }
            else
            {
                var existingEndArea = await dbContext.Areas.FindAsync(dbArtist.EndArea.Id);
                if (existingEndArea != null)
                {
                    dbContext.Entry(existingEndArea).CurrentValues.SetValues(dbArtist.EndArea);
                    dbArtist.EndArea = existingEndArea;
                }
                else
                {
                    dbContext.Areas.Add(dbArtist.EndArea);
                }
            }
        }

        dbContext.Artists.Add(dbArtist);
        await dbContext.SaveChangesAsync();
        MarkAsComplete();
    }

    public Task Handle(AddArtistToServerLibrarySagaEvents.DidNotFindArtistInMusicBrainz message)
    {
        MarkAsComplete();
        return Task.CompletedTask;
    }
}
