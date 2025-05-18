using AutoMapper;
using HotChocolate.Subscriptions;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup.Sagas;

public class AddReleaseGroupToServerLibrarySaga(
    IBus bus,
    ITopicEventSender sender,
    EventDbContext dbContext,
    ILogger<AddReleaseGroupToServerLibrarySaga> logger,
    IMapper mapper,
    MusicBrainzService musicBrainzService
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

        var existingReleaseGroup = await dbContext.ReleaseGroups.FirstOrDefaultAsync(a =>
            a.Id == message.ReleaseGroupMbId
        );

        if (existingReleaseGroup is not null)
        {
            logger.LogInformation(
                "ReleaseGroup {ReleaseGroupMbId} is already in the library",
                message.ReleaseGroupMbId
            );
            MarkAsComplete();
            return;
        }

        Data.StatusDescription = "Looking up release group";
        await bus.Send(
            new AddReleaseGroupToServerLibrarySagaEvents.FindReleaseGroupInMusicBrainz(
                new(message.ReleaseGroupMbId)
            )
        );
    }

    public async Task Handle(
        AddReleaseGroupToServerLibrarySagaEvents.FoundReleaseGroupInMusicBrainz message
    )
    {
        logger.LogInformation(
            $"Saving release group {message.ReleaseGroupMbId} to library database"
        );

        try
        {
            var rgDto = message.ReleaseGroup;

            var rgEntity = await dbContext.ReleaseGroups.FindAsync(message.ReleaseGroupMbId);
            bool isNewReleaseGroup = rgEntity == null;

            if (isNewReleaseGroup)
            {
                logger.LogInformation(
                    $"ReleaseGroup {message.ReleaseGroupMbId} not found, creating new."
                );
                rgEntity = mapper.Map<Db.Models.ServerLibrary.MusicMetaData.ReleaseGroup>(rgDto);
            }
            else
            {
                logger.LogInformation(
                    $"ReleaseGroup {message.ReleaseGroupMbId} found, updating existing."
                );
                mapper.Map(rgDto, rgEntity);
            }

            if (rgEntity.Credits != null)
            {
                foreach (var nameCredit in rgEntity.Credits)
                {
                    if (nameCredit.Artist != null)
                    {
                        var artistDataFromDto = nameCredit.Artist;
                        Db.Models.ServerLibrary.MusicMetaData.Artist? existingArtistInDb = await dbContext.Artists.FindAsync(artistDataFromDto.Id);
                        Db.Models.ServerLibrary.MusicMetaData.Artist artistToProcess;

                        if (existingArtistInDb != null)
                        {
                            mapper.Map(artistDataFromDto, existingArtistInDb);
                            artistToProcess = existingArtistInDb;
                        }
                        else
                        {
                            artistToProcess = artistDataFromDto;
                            var entry = dbContext.Entry(artistToProcess);
                            if (entry.State == EntityState.Detached)
                            {
                                dbContext.Artists.Add(artistToProcess);
                            }
                        }
                        nameCredit.Artist = artistToProcess;
                    }
                }
            }

            dbContext.AttachKnownEntities(rgEntity);

            if (isNewReleaseGroup)
            {
                dbContext.Entry(rgEntity).State = EntityState.Added;
            }
            else
            {
                dbContext.Entry(rgEntity).State = EntityState.Modified;
            }

            // --- Fetch and Process Releases for the Release Group ---
            var releaseDtos = await musicBrainzService.GetReleasesForReleaseGroupAsync(rgEntity.Id);

            if (releaseDtos != null)
            {
                foreach (var releaseDto in releaseDtos)
                {
                    var releaseEntity = await dbContext.Releases.Include(r => r.Media).ThenInclude(m => m.Tracks).ThenInclude(t => t.Recording).FirstOrDefaultAsync(r => r.Id == releaseDto.Id);
                    bool isNewRelease = releaseEntity == null;

                    if (isNewRelease)
                    {
                        logger.LogInformation($"Release {releaseDto.Id} for RG {rgEntity.Id} not found, creating new.");
                        releaseEntity = mapper.Map<Db.Models.ServerLibrary.MusicMetaData.Release>(releaseDto);
                        releaseEntity.ReleaseGroup = rgEntity; 
                    }
                    else
                    {
                        logger.LogInformation($"Release {releaseDto.Id} for RG {rgEntity.Id} found, updating existing.");
                        if (releaseEntity.ReleaseGroup == null || releaseEntity.ReleaseGroup.Id != rgEntity.Id) {
                             releaseEntity.ReleaseGroup = rgEntity;
                        }
                        mapper.Map(releaseDto, releaseEntity);
                    }

                    if (releaseEntity.Credits != null)
                    {
                        foreach (var nameCredit in releaseEntity.Credits)
                        {
                            if (nameCredit.Artist != null)
                            {
                                var artistDataFromDto = nameCredit.Artist;
                                Db.Models.ServerLibrary.MusicMetaData.Artist? existingArtistInDb = await dbContext.Artists.FindAsync(artistDataFromDto.Id);
                                Db.Models.ServerLibrary.MusicMetaData.Artist artistToProcess;

                                if (existingArtistInDb != null)
                                {
                                    mapper.Map(artistDataFromDto, existingArtistInDb);
                                    artistToProcess = existingArtistInDb;
                                }
                                else
                                {
                                    artistToProcess = artistDataFromDto;
                                    var entry = dbContext.Entry(artistToProcess);
                                    if (entry.State == EntityState.Detached)
                                    {
                                        dbContext.Artists.Add(artistToProcess);
                                    }
                                }
                                nameCredit.Artist = artistToProcess;
                            }
                        }
                    }
                    
                    if (releaseEntity.Media != null)
                    {
                        foreach (var mediumEntity in releaseEntity.Media)
                        {
                            if (mediumEntity.Tracks != null)
                            {
                                foreach (var trackEntity in mediumEntity.Tracks)
                                {
                                    if (trackEntity.Recording != null)
                                    {
                                        var recordingDataFromDto = trackEntity.Recording; // This is the mapped Recording entity from Release DTO
                                        var recordingEntity = await dbContext.Recordings.FindAsync(recordingDataFromDto.Id);
                                        bool isNewRecording = recordingEntity == null;

                                        if (isNewRecording)
                                        {
                                            logger.LogInformation($"Recording {recordingDataFromDto.Id} for Release {releaseEntity.Id} not found, creating new.");
                                            recordingEntity = recordingDataFromDto; // Assumes it's already fully mapped by AutoMapper
                                        }
                                        else
                                        {
                                            logger.LogInformation($"Recording {recordingDataFromDto.Id} for Release {releaseEntity.Id} found, updating existing.");
                                            mapper.Map(recordingDataFromDto, recordingEntity); 
                                            trackEntity.Recording = recordingEntity; // Ensure track points to tracked instance
                                        }

                                        if (recordingEntity.Credits != null)
                                        {
                                            foreach (var nameCredit in recordingEntity.Credits)
                                            {
                                                 if (nameCredit.Artist != null)
                                                 {
                                                    var artistDataFromDto = nameCredit.Artist;
                                                    Db.Models.ServerLibrary.MusicMetaData.Artist? existingArtistInDb = await dbContext.Artists.FindAsync(artistDataFromDto.Id);
                                                    Db.Models.ServerLibrary.MusicMetaData.Artist artistToProcess;

                                                    if (existingArtistInDb != null)
                                                    {
                                                        mapper.Map(artistDataFromDto, existingArtistInDb);
                                                        artistToProcess = existingArtistInDb;
                                                    }
                                                    else
                                                    {
                                                        artistToProcess = artistDataFromDto;
                                                        var entry = dbContext.Entry(artistToProcess);
                                                        if (entry.State == EntityState.Detached)
                                                        {
                                                            dbContext.Artists.Add(artistToProcess);
                                                        }
                                                    }
                                                    nameCredit.Artist = artistToProcess;
                                                 }
                                            }
                                        }
                                        
                                        dbContext.AttachKnownEntities(recordingEntity);
                                        dbContext.Entry(recordingEntity).State = isNewRecording ? EntityState.Added : EntityState.Modified;
                                    }
                                }
                            }
                        }
                    }

                    dbContext.AttachKnownEntities(releaseEntity);
                    dbContext.Entry(releaseEntity).State = isNewRelease ? EntityState.Added : EntityState.Modified;
                }
            }

            await dbContext.SaveChangesAsync();
            logger.LogInformation($"Successfully saved ReleaseGroup {message.ReleaseGroupMbId}");
            MarkAsComplete();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                $"Error saving ReleaseGroup {message.ReleaseGroupMbId} to library database"
            );
            MarkAsComplete();
        }
    }

    public Task Handle(
        AddReleaseGroupToServerLibrarySagaEvents.DidNotFindReleaseGroupInMusicBrainz message
    )
    {
        logger.LogWarning(
            "Did not find ReleaseGroup {ReleaseGroupMbId} in MusicBrainz. Completing saga.",
            message.ReleaseGroupMbId
        );
        MarkAsComplete();
        return Task.CompletedTask;
    }
}
