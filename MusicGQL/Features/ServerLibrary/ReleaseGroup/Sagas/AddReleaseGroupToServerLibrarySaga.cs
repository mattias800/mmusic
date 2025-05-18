using AutoMapper;
using MusicGQL.Common;
using MusicGQL.Integration.MusicBrainz;
using Neo4j.Driver;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup.Sagas;

public class AddReleaseGroupToServerLibrarySaga(
    IBus bus,
    IDriver driver,
    ILogger<AddReleaseGroupToServerLibrarySaga> logger,
    IMapper mapper,
    MusicBrainzService musicBrainzService,
    ReleaseGroupPersistenceService releaseGroupPersistenceService
)
    : Saga<AddReleaseGroupToServerLibrarySagaData>,
        IAmInitiatedBy<AddReleaseGroupToServerLibrarySagaEvents.StartAddReleaseGroup>,
        IHandleMessages<AddReleaseGroupToServerLibrarySagaEvents.FoundReleaseGroupInMusicBrainz>,
        IHandleMessages<AddReleaseGroupToServerLibrarySagaEvents.DidNotFindReleaseGroupInMusicBrainz>
{
    protected override void CorrelateMessages(
        ICorrelationConfig<AddReleaseGroupToServerLibrarySagaData> config
    )
    {
        config.Correlate<AddReleaseGroupToServerLibrarySagaEvents.StartAddReleaseGroup>(
            m => m.ReleaseGroupMbId,
            s => s.ReleaseGroupMbId
        );
        config.Correlate<AddReleaseGroupToServerLibrarySagaEvents.FindReleaseGroupInMusicBrainz>(
            m => m.ReleaseGroupMbId,
            s => s.ReleaseGroupMbId
        );
        config.Correlate<AddReleaseGroupToServerLibrarySagaEvents.FoundReleaseGroupInMusicBrainz>(
            m => m.ReleaseGroupMbId,
            s => s.ReleaseGroupMbId
        );
        config.Correlate<AddReleaseGroupToServerLibrarySagaEvents.DidNotFindReleaseGroupInMusicBrainz>(
            m => m.ReleaseGroupMbId,
            s => s.ReleaseGroupMbId
        );
    }

    public async Task Handle(AddReleaseGroupToServerLibrarySagaEvents.StartAddReleaseGroup message)
    {
        if (!IsNew)
        {
            return;
        }

        logger.LogInformation(
            "Starting AddReleaseGroupToServerLibrarySaga for {ReleaseGroupMbId}",
            message.ReleaseGroupMbId
        );

        Data.StatusDescription = "Looking up release group in MusicBrainz";
        await bus.Send(
            new AddReleaseGroupToServerLibrarySagaEvents.FindReleaseGroupInMusicBrainz(
                message.ReleaseGroupMbId
            )
        );
    }

    public async Task Handle(
        AddReleaseGroupToServerLibrarySagaEvents.FoundReleaseGroupInMusicBrainz message
    )
    {
        var releaseGroupDto = message.ReleaseGroup;

        logger.LogInformation(
            "Fetching all releases for release group {MbId}",
            message.ReleaseGroupMbId
        );

        var releaseDtos = await musicBrainzService.GetReleasesForReleaseGroupAsync(
            releaseGroupDto.Id
        );

        logger.LogInformation(
            "Fetched {ReleaseCount} releases for release group {MbId}",
            releaseDtos.Count,
            releaseGroupDto.Id
        );

        var mainRelease = MainAlbumFinder.GetMainReleaseInReleaseGroup(releaseDtos);

        logger.LogInformation(
            "Persisting all data for release group {MbId}",
            message.ReleaseGroupMbId
        );

        await using var session = driver.AsyncSession();

        try
        {
            await session.ExecuteWriteAsync(async tx =>
            {
                // 1. Save release group itself
                var releaseGroupToSave =
                    mapper.Map<Db.Neo4j.ServerLibrary.MusicMetaData.ReleaseGroup>(releaseGroupDto);

                await releaseGroupPersistenceService.SaveReleaseGroupNodeAsync(
                    (IAsyncTransaction)tx,
                    releaseGroupToSave
                );

                logger.LogInformation("Saved release group {Title}", releaseGroupToSave.Title);

                // 2. Save ReleaseGroup Artist Credits
                if (releaseGroupDto.Credits != null)
                {
                    await releaseGroupPersistenceService.SaveArtistCreditsForParentAsync(
                        (IAsyncTransaction)tx,
                        releaseGroupToSave.Id,
                        releaseGroupDto.Credits,
                        "ReleaseGroup",
                        "rgId",
                        "CREDITED_ON_RELEASE_GROUP"
                    );
                    logger.LogInformation(
                        "Saved artist credits for release group {Title}",
                        releaseGroupToSave.Title
                    );
                }

                var releaseToSave = mapper.Map<Db.Neo4j.ServerLibrary.MusicMetaData.Release>(
                    mainRelease
                );
                await releaseGroupPersistenceService.SaveReleaseNodeAsync(
                    (IAsyncTransaction)tx,
                    releaseToSave
                );
                await releaseGroupPersistenceService.LinkReleaseToReleaseGroupAsync(
                    (IAsyncTransaction)tx,
                    releaseGroupToSave.Id,
                    releaseToSave.Id
                );
                logger.LogInformation(
                    "Saved main release {ReleaseTitle} and linked to release group {ReleaseGroupTitle}",
                    releaseToSave.Title,
                    releaseGroupToSave.Title
                );

                // 4. Save Release Artist Credits
                if (mainRelease.Credits != null)
                {
                    await releaseGroupPersistenceService.SaveArtistCreditsForParentAsync(
                        (IAsyncTransaction)tx,
                        releaseToSave.Id,
                        mainRelease.Credits,
                        "Release",
                        "releaseId",
                        "CREDITED_ON_RELEASE"
                    );
                    logger.LogInformation(
                        "Saved artist credits for release {Title}",
                        releaseToSave.Title
                    );
                }

                // 5. Process Media and Tracks for each Release
                if (mainRelease.Media != null)
                {
                    foreach (var mediumDto in mainRelease.Media)
                    {
                        if (mediumDto.Tracks != null)
                        {
                            foreach (var trackDto in mediumDto.Tracks)
                            {
                                if (
                                    trackDto.Recording == null
                                    || string.IsNullOrEmpty(trackDto.Recording.Id)
                                )
                                    continue;

                                var recordingToSave =
                                    mapper.Map<Db.Neo4j.ServerLibrary.MusicMetaData.Recording>(
                                        trackDto.Recording
                                    );

                                await releaseGroupPersistenceService.SaveRecordingNodeAsync(
                                    (IAsyncTransaction)tx,
                                    recordingToSave
                                );

                                await releaseGroupPersistenceService.LinkRecordingToReleaseAsync(
                                    (IAsyncTransaction)tx,
                                    releaseToSave.Id,
                                    recordingToSave.Id,
                                    trackDto,
                                    mediumDto
                                );

                                logger.LogInformation(
                                    "Saved recording {RecordingTitle} and linked to Release {ReleaseTitle}",
                                    recordingToSave.Title,
                                    releaseToSave.Title
                                );

                                // 6. Save Recording Artist Credits
                                if (trackDto.Recording.Credits != null)
                                {
                                    await releaseGroupPersistenceService.SaveArtistCreditsForParentAsync(
                                        (IAsyncTransaction)tx,
                                        recordingToSave.Id,
                                        trackDto.Recording.Credits,
                                        "Recording",
                                        "recordingId",
                                        "CREDITED_ON_RECORDING"
                                    );
                                    logger.LogInformation(
                                        "Saved artist credits for recording {RecordingTitle}",
                                        recordingToSave.Title
                                    );
                                }
                            }
                        }
                    }
                }
            });

            logger.LogInformation(
                "Successfully persisted all data for release group {Title}",
                message.ReleaseGroup.Title
            );
            MarkAsComplete();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error persisting data for release group {Title}: {ExMessage}",
                message.ReleaseGroup.Title,
                ex.Message
            );
            MarkAsComplete(); // Ensure saga completes even on error
        }
    }

    public Task Handle(
        AddReleaseGroupToServerLibrarySagaEvents.DidNotFindReleaseGroupInMusicBrainz message
    )
    {
        logger.LogInformation(
            "Release group {MessageReleaseGroupMbId} not found in MusicBrainz, completing saga",
            message.ReleaseGroupMbId
        );
        MarkAsComplete();
        return Task.CompletedTask;
    }
}
