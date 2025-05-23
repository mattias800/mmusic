using AutoMapper;
using HotChocolate.Subscriptions;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Handlers;
using MusicGQL.Integration.Neo4j;
using Neo4j.Driver;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace MusicGQL.Features.ServerLibrary.Artist.Sagas;

public class AddArtistToServerLibrarySaga(
    IBus bus,
    ITopicEventSender sender,
    IDriver driver,
    ILogger<AddArtistToServerLibrarySaga> logger,
    IMapper mapper,
    MarkReleaseGroupAsAddedToServerLibraryHandler markReleaseGroupAsAddedToServerLibraryHandler,
    Neo4jPersistenceService neo4JPersistenceService
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
            new AddArtistToServerLibrarySagaEvents.FindArtistInMusicBrainz(message.ArtistMbId)
        );
    }

    public async Task Handle(AddArtistToServerLibrarySagaEvents.FoundArtistInMusicBrainz message)
    {
        logger.LogInformation(
            "Processing artist {ArtistMbId} for persistence and handling associated release groups",
            message.Artist.Id
        );

        try
        {
            await using var session = driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await neo4JPersistenceService.SaveArtistNodeAsync(
                    (IAsyncTransaction)tx,
                    message.Artist
                );
            });

            logger.LogInformation("Artist {ArtistMbId} saved/updated in Neo4j", message.Artist.Id);

            var releaseGroupIds = message
                .ReleaseGroups.Where(LibraryDecider.ShouldBeAddedWhenAddingArtistToServerLibrary)
                .Select(r => r.Id)
                .ToList();

            logger.LogInformation(
                "Found {Count} release groups associated with artist {ArtistMbId}. Marking them...",
                releaseGroupIds.Count,
                message.Artist.Id
            );

            foreach (var releaseGroupId in releaseGroupIds)
            {
                await markReleaseGroupAsAddedToServerLibraryHandler.Handle(new(releaseGroupId));
            }

            MarkAsComplete();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving artist to Neo4j and processing release groups");
            MarkAsComplete();
        }
    }

    public Task Handle(AddArtistToServerLibrarySagaEvents.DidNotFindArtistInMusicBrainz message)
    {
        logger.LogInformation(
            "Artist {ArtistMbId} not found in MusicBrainz, completing saga",
            message.ArtistMbId
        );
        MarkAsComplete();
        return Task.CompletedTask;
    }
}
