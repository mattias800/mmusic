using AutoMapper;
using Hqub.MusicBrainz.Entities;
using MusicGQL.Integration.MusicBrainz;
using Neo4j.Driver;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup.Sagas;

public class AddReleaseGroupToServerLibrarySaga(
    IBus bus,
    IDriver neo4jDriver,
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
        logger.LogInformation(
            $"Processing ReleaseGroup {message.ReleaseGroupMbId} and associated data for Neo4j."
        );

        await using var session = neo4jDriver.AsyncSession();

        try
        {
            await SaveReleaseGroupAndItsArtistsAsync(session, message.ReleaseGroup);

            await ProcessAllReleasesForReleaseGroupAsync(session, message.ReleaseGroup.Id);

            MarkAsComplete();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error processing ReleaseGroup {MessageReleaseGroupMbId} for Neo4j: {ExMessage}",
                message.ReleaseGroupMbId,
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
            $"ReleaseGroup {message.ReleaseGroupMbId} not found in MusicBrainz, completing saga."
        );
        MarkAsComplete();
        return Task.CompletedTask;
    }

    private async Task SaveReleaseGroupAndItsArtistsAsync(
        IAsyncSession session,
        Hqub.MusicBrainz.Entities.ReleaseGroup releaseGroupDto
    )
    {
        var releaseGroupToSave = mapper.Map<Db.Neo4j.ServerLibrary.MusicMetaData.ReleaseGroup>(
            releaseGroupDto
        );
        await session.ExecuteWriteAsync(async tx =>
        {
            await releaseGroupPersistenceService.SaveReleaseGroupNodeAsync(
                (IAsyncTransaction)tx,
                releaseGroupToSave
            );

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
            }
        });
        logger.LogInformation(
            $"ReleaseGroup {releaseGroupToSave.Id} and its main artist credits saved/updated in Neo4j"
        );
    }

    private async Task ProcessAllReleasesForReleaseGroupAsync(
        IAsyncSession session,
        string releaseGroupMbId
    )
    {
        var releaseDtos = await musicBrainzService.GetReleasesForReleaseGroupAsync(
            releaseGroupMbId
        );
        if (releaseDtos != null)
        {
            foreach (var releaseDto in releaseDtos)
            {
                await ProcessSingleReleaseAsync(session, releaseDto, releaseGroupMbId);
            }
        }
    }

    private async Task ProcessSingleReleaseAsync(
        IAsyncSession session,
        Hqub.MusicBrainz.Entities.Release releaseDto,
        string releaseGroupMbId
    )
    {
        var releaseToSave = mapper.Map<Db.Neo4j.ServerLibrary.MusicMetaData.Release>(releaseDto);
        await session.ExecuteWriteAsync(async tx =>
        {
            await releaseGroupPersistenceService.SaveReleaseNodeAsync(
                (IAsyncTransaction)tx,
                releaseToSave
            );
            await releaseGroupPersistenceService.LinkReleaseToReleaseGroupAsync(
                (IAsyncTransaction)tx,
                releaseGroupMbId,
                releaseToSave.Id
            );

            if (releaseDto.Credits != null)
            {
                await releaseGroupPersistenceService.SaveArtistCreditsForParentAsync(
                    (IAsyncTransaction)tx,
                    releaseToSave.Id,
                    releaseDto.Credits,
                    "Release",
                    "releaseId",
                    "CREDITED_ON_RELEASE"
                );
            }

            if (releaseDto.Media != null)
            {
                await ProcessMediaAndTracksForReleaseAsync(
                    (IAsyncTransaction)tx,
                    releaseDto,
                    releaseToSave.Id
                );
            }
        });
        logger.LogInformation($"Release {releaseToSave.Id} and its data saved/updated in Neo4j");
    }

    private async Task ProcessMediaAndTracksForReleaseAsync(
        IAsyncTransaction tx,
        Hqub.MusicBrainz.Entities.Release releaseDto,
        string releaseId
    )
    {
        if (releaseDto.Media == null)
            return;

        foreach (var mediumDto in releaseDto.Media)
        {
            if (mediumDto.Tracks != null)
            {
                foreach (var trackDto in mediumDto.Tracks)
                {
                    await ProcessSingleTrackAsync(tx, trackDto, mediumDto, releaseId);
                }
            }
        }
    }

    private async Task ProcessSingleTrackAsync(
        IAsyncTransaction tx,
        Track trackDto,
        Medium mediumDto,
        string releaseId
    )
    {
        if (trackDto.Recording == null || string.IsNullOrEmpty(trackDto.Recording.Id))
            return;

        var recordingToSave = mapper.Map<Db.Neo4j.ServerLibrary.MusicMetaData.Recording>(
            trackDto.Recording
        );
        await releaseGroupPersistenceService.SaveRecordingNodeAsync(tx, recordingToSave);
        await releaseGroupPersistenceService.LinkRecordingToReleaseAsync(
            tx,
            releaseId,
            recordingToSave.Id,
            trackDto,
            mediumDto
        );

        if (trackDto.Recording.Credits != null)
        {
            await releaseGroupPersistenceService.SaveArtistCreditsForParentAsync(
                tx,
                recordingToSave.Id,
                trackDto.Recording.Credits,
                "Recording",
                "recordingId",
                "CREDITED_ON_RECORDING"
            );
        }
    }
}
