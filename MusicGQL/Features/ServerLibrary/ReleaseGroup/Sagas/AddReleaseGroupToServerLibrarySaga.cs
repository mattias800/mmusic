using AutoMapper;
using HotChocolate.Subscriptions;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup.Sagas;

public class AddReleaseGroupToServerLibrarySaga(
    IBus bus,
    ITopicEventSender sender,
    EventDbContext dbContext,
    ILogger<AddReleaseGroupToServerLibrarySaga> logger,
    IMapper mapper
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

        logger.LogInformation("Starting AddReleaseGroupToServerLibrarySaga");

        var existingReleaseGroup = await dbContext.ReleaseGroups.FirstOrDefaultAsync(a =>
            a.Id == message.ReleaseGroupMbId
        );

        if (existingReleaseGroup is not null)
        {
            logger.LogInformation("ReleaseGroup is already in the library");

            MarkAsComplete();
            return;
        }

        Data.StatusDescription = "Looking up release";

        // await sender.SendAsync(nameof(DownloadSubscription.DownloadStarted), Data);

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
        logger.LogInformation($"Saving release group {message.ReleaseGroupMbId} to library database");

        try
        {
            var rgDto = message.ReleaseGroup; // This is the single Hqub.MusicBrainz.Entities.ReleaseGroup DTO

            // --- Main ReleaseGroup Entity Handling ---
            var rgEntity = await dbContext.ReleaseGroups.FindAsync(message.ReleaseGroupMbId);
            bool isNewReleaseGroup = rgEntity == null;

            if (isNewReleaseGroup)
            {
                logger.LogInformation($"ReleaseGroup {message.ReleaseGroupMbId} not found, creating new.");
                rgEntity = mapper.Map<Db.Models.ServerLibrary.MusicMetaData.ReleaseGroup>(rgDto);
                // Note: We will explicitly set state to Added later, after AttachKnownEntities
            }
            else
            {
                logger.LogInformation($"ReleaseGroup {message.ReleaseGroupMbId} found, updating existing.");
                mapper.Map(rgDto, rgEntity); // Update properties of the tracked rgEntity
            }

            // --- Artist Credits Handling (Stitching/Adding Artists) ---
            if (rgEntity.Credits != null)
            {
                foreach (var nameCredit in rgEntity.Credits)
                {
                    if (nameCredit.Artist != null)
                    {
                        // artistInCredit is the EF model instance, mapped by AutoMapper from the Hqub DTO graph.
                        var artistInCredit = nameCredit.Artist; 
                        var existingArtist = await dbContext.Artists.FindAsync(artistInCredit.Id);

                        if (existingArtist != null)
                        {
                            // Artist already exists in DB. Update it and make sure NameCredit points to the tracked one.
                            mapper.Map(artistInCredit, existingArtist); // Update properties of existing tracked entity
                            nameCredit.Artist = existingArtist; // Stitch: nameCredit now points to the tracked DB entity
                        }
                        else
                        {
                            // Artist does not exist in DB. The artistInCredit is a new entity.
                            // Check if it's already tracked (e.g. if multiple credits point to the same new artist object)
                            var entry = dbContext.Entry(artistInCredit);
                            if (entry.State == EntityState.Detached)
                            {
                                // It's a new artist and not yet tracked, so add it explicitly.
                                // This ensures its sub-graph (Area, etc.) is also processed by AttachKnownEntities if not already handled.
                                dbContext.Artists.Add(artistInCredit);
                            }
                            // If state is not Detached, it means it's already tracked (e.g., Added, Unchanged from a previous Add call for the same instance)
                            // nameCredit.Artist already points to this new, now-tracked or soon-to-be-added artistInCredit instance.
                        }
                    }
                }
            }

            // --- Rating Handling ---
            if (rgEntity.Rating != null)
            {
                var mappedRating = rgEntity.Rating;
                if (mappedRating.Id != 0) // Existing Rating ID from source
                {
                    var existingRating = await dbContext.Ratings.FindAsync(mappedRating.Id);
                    if (existingRating != null)
                    {
                        mapper.Map(mappedRating, existingRating);
                        rgEntity.Rating = existingRating;
                    }
                    else
                    {
                        logger.LogWarning($"ReleaseGroup {rgEntity.Id} references RatingId {mappedRating.Id} which does not exist. Nullifying Rating.");
                        rgEntity.Rating = null; // Assumes ReleaseGroup.RatingId is nullable
                    }
                }
                else // New Rating (Id = 0)
                {
                    // Explicitly add new Rating. AttachKnownEntities might not handle it correctly for FKs.
                    dbContext.Ratings.Add(mappedRating);
                }
            }

            // --- Area Handling (Example, if ReleaseGroup has a direct Area) ---
            // if (rgEntity.Area != null) { /* Similar logic as Rating */ }

            // Attach the main rgEntity and its potentially modified/newly related graph parts.
            dbContext.AttachKnownEntities(rgEntity);

            // Explicitly set state for the main ReleaseGroup entity
            if (isNewReleaseGroup)
            {
                dbContext.Entry(rgEntity).State = EntityState.Added;
            }
            else
            {
                dbContext.Entry(rgEntity).State = EntityState.Modified;
            }

            await dbContext.SaveChangesAsync();
            logger.LogInformation($"Successfully saved ReleaseGroup {message.ReleaseGroupMbId}");
            MarkAsComplete();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error saving ReleaseGroup {message.ReleaseGroupMbId} to library database");
            MarkAsComplete(); // Ensure saga completes even on error
        }
    }

    public Task Handle(
        AddReleaseGroupToServerLibrarySagaEvents.DidNotFindReleaseGroupInMusicBrainz message
    )
    {
        MarkAsComplete();
        return Task.CompletedTask;
    }
}
