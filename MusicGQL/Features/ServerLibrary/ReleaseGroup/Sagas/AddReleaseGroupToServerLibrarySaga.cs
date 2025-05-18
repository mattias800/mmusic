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

        if (releaseGroupDto == null)
        {
            logger.LogWarning(
                "Received null ReleaseGroup DTO for MbId {MbId}. Saga will not proceed with persistence",
                message.ReleaseGroupMbId
            );
            MarkAsComplete();
            return;
        }

        logger.LogInformation("Fetching all releases for release group {MbId}", releaseGroupDto.Id);

        var allReleaseDtos = await musicBrainzService.GetReleasesForReleaseGroupAsync(
            releaseGroupDto.Id
        );

        logger.LogInformation(
            "Fetched {ReleaseCount} releases for release group {Title}",
            allReleaseDtos.Count,
            releaseGroupDto.Title
        );

        var mainRelease = MainAlbumFinder.GetMainReleaseInReleaseGroup(allReleaseDtos.ToList());

        if (mainRelease == null)
        {
            logger.LogWarning(
                "No main release found for release group {Title} after fetching {Count} releases. Only the release group itself will be persisted",
                releaseGroupDto.Title,
                allReleaseDtos.Count
            );
        }
        else
        {
            logger.LogInformation(
                "Prioritized main release for release group {RgTitle} is {MainReleaseTitle} ({MainReleaseId})",
                releaseGroupDto.Title,
                mainRelease.Title,
                mainRelease.Id
            );
        }

        logger.LogInformation(
            "Persisting data for release group {MbId} (and its main release if found)",
            releaseGroupDto.Id
        );

        await using var session = driver.AsyncSession();
        try
        {
            await session.ExecuteWriteAsync(async tx =>
            {
                // 1. Save release group itself
                var rgToSave = mapper.Map<Db.Neo4j.ServerLibrary.MusicMetaData.ReleaseGroup>(
                    releaseGroupDto
                );
                await releaseGroupPersistenceService.SaveReleaseGroupNodeAsync(
                    (IAsyncTransaction)tx,
                    rgToSave
                );
                logger.LogInformation("Saved release group {Title}", rgToSave.Title);

                // 2. Save ReleaseGroup Artist Credits
                if (releaseGroupDto.Credits != null)
                {
                    await releaseGroupPersistenceService.SaveArtistCreditsForParentAsync(
                        (IAsyncTransaction)tx,
                        rgToSave.Id,
                        releaseGroupDto.Credits,
                        "ReleaseGroup",
                        "rgId",
                        "CREDITED_ON_RELEASE_GROUP"
                    );
                    logger.LogInformation(
                        "Saved artist credits for release group {Title}",
                        rgToSave.Title
                    );
                }

                // 3. Process ONLY the Main Release (if found)
                if (mainRelease != null)
                {
                    var releaseToSave = mapper.Map<Db.Neo4j.ServerLibrary.MusicMetaData.Release>(
                        mainRelease
                    );
                    await releaseGroupPersistenceService.SaveReleaseNodeAsync(
                        (IAsyncTransaction)tx,
                        releaseToSave
                    );
                    await releaseGroupPersistenceService.LinkReleaseToReleaseGroupAsync(
                        (IAsyncTransaction)tx,
                        rgToSave.Id,
                        releaseToSave.Id
                    );
                    logger.LogInformation(
                        "Saved Main Release {ReleaseTitle} ({ReleaseId}) and linked to RG {RgTitle}",
                        releaseToSave.Title,
                        releaseToSave.Id,
                        rgToSave.Title
                    );

                    // 4. Save Main Release Artist Credits
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
                            "Saved artist credits for main release {Title}",
                            releaseToSave.Title
                        );
                    }

                    // 5. Process Media and Tracks for the Main Release
                    if (mainRelease.Media != null)
                    {
                        foreach (var mediumDto in mainRelease.Media)
                        {
                            if (mediumDto == null)
                                continue;

                            string mediumNodeId = $"{releaseToSave.Id}_m{mediumDto.Position}";
                            await releaseGroupPersistenceService.SaveMediumNodeAsync(
                                (IAsyncTransaction)tx,
                                mediumNodeId,
                                releaseToSave.Id,
                                mediumDto
                            );
                            logger.LogInformation(
                                "Saved Medium {MediumId} (Pos: {MediumPos}) for Main Release {ReleaseTitle}",
                                mediumNodeId,
                                mediumDto.Position,
                                releaseToSave.Title
                            );

                            if (mediumDto.Tracks != null)
                            {
                                foreach (var trackDto in mediumDto.Tracks)
                                {
                                    if (
                                        trackDto?.Recording == null
                                        || string.IsNullOrEmpty(trackDto.Recording.Id)
                                    )
                                        continue;

                                    var recordingToSave =
                                        mapper.Map<Db.Neo4j.ServerLibrary.MusicMetaData.Recording>(
                                            trackDto.Recording
                                        );
                                    if (recordingToSave == null)
                                    {
                                        logger.LogWarning(
                                            "Mapping Recording DTO for Track {TrackPos} on Medium {MediumPos} resulted in a null object. Skipping track",
                                            trackDto.Position,
                                            mediumDto.Position
                                        );
                                        continue;
                                    }

                                    await releaseGroupPersistenceService.SaveRecordingNodeAsync(
                                        (IAsyncTransaction)tx,
                                        recordingToSave
                                    );

                                    await releaseGroupPersistenceService.LinkTrackOnMediumToRecordingAsync(
                                        (IAsyncTransaction)tx,
                                        mediumNodeId,
                                        recordingToSave.Id,
                                        trackDto
                                    );
                                    logger.LogInformation(
                                        "Linked Track (Pos:{TrackPos}, Num:{TrackNum}, Title:'{TrackTitle}') on Medium {MediumId} to Recording {RecordingTitle} ({RecordingId})",
                                        trackDto.Position,
                                        trackDto.Number,
                                        trackDto.Recording?.Title ?? string.Empty,
                                        mediumNodeId,
                                        recordingToSave?.Title ?? string.Empty,
                                        recordingToSave?.Id ?? string.Empty
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
                                            recordingToSave?.Title ?? string.Empty
                                        );
                                    }
                                }
                            }
                        }
                    }
                }
            });

            logger.LogInformation(
                "Successfully persisted data for release group {Title}",
                releaseGroupDto.Title
            );
            MarkAsComplete();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error persisting data for release group {Title}: {ExMessage}",
                releaseGroupDto.Title,
                ex.Message
            );
            MarkAsComplete();
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
