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
