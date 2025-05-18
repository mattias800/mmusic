using AutoMapper;
using HotChocolate.Subscriptions;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Handlers;
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
            new AddArtistToServerLibrarySagaEvents.FindArtistInMusicBrainz(message.ArtistMbId)
        );
    }

    public async Task Handle(AddArtistToServerLibrarySagaEvents.FoundArtistInMusicBrainz message)
    {
        logger.LogInformation(
            "Saving artist to Neo4j and marking release groups as added to library database"
        );

        try
        {
            // --- Artist Handling with Neo4j ---
            var artistToSave = mapper.Map<Db.Neo4j.ServerLibrary.MusicMetaData.Artist>(
                message.Artist
            );

            await using var session = driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    "MERGE (a:Artist {Id: $id}) "
                        + "ON CREATE SET a.Name = $name, a.SortName = $sortName, a.Gender = $gender "
                        + "ON MATCH SET a.Name = $name, a.SortName = $sortName, a.Gender = $gender",
                    new
                    {
                        id = artistToSave.Id,
                        name = artistToSave.Name,
                        sortName = artistToSave.SortName,
                        gender = artistToSave.Gender,
                    }
                );
            });

            logger.LogInformation("Artist {ArtistMbId} saved/updated in Neo4j", artistToSave.Id);

            var releaseGroupIds = message.ReleaseGroups.Select(r => r.Id).ToList();

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
            "Artist {ArtistMbId} not found in MusicBrainz, completing saga.",
            message.ArtistMbId
        );
        MarkAsComplete();
        return Task.CompletedTask;
    }
}
