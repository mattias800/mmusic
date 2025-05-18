using AutoMapper;
using HotChocolate.Subscriptions;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Handlers;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace MusicGQL.Features.ServerLibrary.Artist.Sagas;

public class AddArtistToServerLibrarySaga(
    IBus bus,
    ITopicEventSender sender,
    EventDbContext dbContext,
    ILogger<AddArtistToServerLibrarySaga> logger,
    MarkReleaseGroupAsAddedToServerLibraryHandler markReleaseGroupAsAddedToServerLibraryHandler,
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
            logger.LogInformation("AddArtistToServerLibrarySaga is not new, skipping");
            return;
        }

        logger.LogInformation("Starting AddArtistToServerLibrarySaga");

        Data.StatusDescription = "Looking up artist";

        // await sender.SendAsync(nameof(DownloadSubscription.DownloadStarted), Data);

        await bus.Send(
            new AddArtistToServerLibrarySagaEvents.FindArtistInMusicBrainz(new(message.ArtistMbId))
        );
    }

    public async Task Handle(AddArtistToServerLibrarySagaEvents.FoundArtistInMusicBrainz message)
    {
        logger.LogInformation(
            "Saving artist and marking release groups as added to library database"
        );

        try
        {
            // --- Artist Handling ---
            var artistEntity = await dbContext.Artists.FirstOrDefaultAsync(a =>
                a.Id == message.ArtistMbId
            );

            if (artistEntity == null)
            {
                logger.LogInformation("Artist not found in library database, creating new one");
                artistEntity = mapper.Map<Db.Models.ServerLibrary.MusicMetaData.Artist>(
                    message.Artist
                );
                dbContext.Artists.Add(artistEntity); // EF Core starts tracking artistEntity
            }
            else
            {
                logger.LogInformation(
                    "Artist already exists in library database, updating existing one"
                );
                mapper.Map(message.Artist, artistEntity); // Update properties of tracked artistEntity
            }

            await dbContext.SaveChangesAsync();

            var releaseGroupIds = message.ReleaseGroups.Select(r => r.Id).ToList();

            foreach (var releaseGroupId in releaseGroupIds)
            {
                await markReleaseGroupAsAddedToServerLibraryHandler.Handle(new(releaseGroupId));
            }

            MarkAsComplete();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving artist and release groups to library database");
            MarkAsComplete();
        }
    }

    public Task Handle(AddArtistToServerLibrarySagaEvents.DidNotFindArtistInMusicBrainz message)
    {
        MarkAsComplete();
        return Task.CompletedTask;
    }
}
