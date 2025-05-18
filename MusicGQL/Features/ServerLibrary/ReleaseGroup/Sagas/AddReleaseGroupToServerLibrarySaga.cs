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
        logger.LogInformation("Saving release group to library database");

        // --- Release Group Handling ---
        var allIncomingRgIds = message.ReleaseGroups.Select(rg => rg.Id).ToHashSet();
        var existingRgIdsInDb = await dbContext
            .ReleaseGroups.AsNoTracking()
            .Where(rg => allIncomingRgIds.Contains(rg.Id))
            .Select(rg => rg.Id)
            .ToHashSetAsync();

        foreach (var rgDto in message.ReleaseGroups)
        {
            var rgEntity = mapper.Map<Db.Models.ServerLibrary.MusicMetaData.ReleaseGroup>(rgDto);

            // Stitch Artist credits
            if (rgEntity.Credits != null)
            {
                foreach (var nameCredit in rgEntity.Credits)
                {
                    if (nameCredit.Artist != null && nameCredit.Artist.Id == artistEntity.Id)
                    {
                        nameCredit.Artist = artistEntity;
                    }
                }
            }

            // --- Explicit Rating Handling before AttachKnownEntities ---
            if (rgEntity.Rating != null)
            {
                var mappedRating = rgEntity.Rating;
                if (mappedRating.Id != 0) // Indicates an existing Rating ID from source
                {
                    var existingRating = await dbContext.Ratings.FindAsync(mappedRating.Id);
                    if (existingRating != null)
                    {
                        // Rating exists in DB, use the tracked instance and update its properties
                        mapper.Map(mappedRating, existingRating); // Update existing with values from DTO
                        rgEntity.Rating = existingRating; // Point RG to the tracked Rating entity
                    }
                    else
                    {
                        // Rating ID specified but not found in DB. This is an FK violation if RatingId is not nullable.
                        // If ReleaseGroup.RatingId is nullable, setting rgEntity.Rating = null is an option.
                        // Otherwise, this is a data integrity issue.
                        logger.LogWarning(
                            $"ReleaseGroup {rgEntity.Id} references RatingId {mappedRating.Id} which does not exist. Setting Rating to null for this RG. This may fail if ReleaseGroup.RatingId is not nullable."
                        );
                        rgEntity.Rating = null;
                    }
                }
                else // mappedRating.Id is 0, treat as a new Rating to be inserted
                {
                    // Add the new Rating to the context. EF will handle its insertion.
                    // If AttachKnownEntities also tries to add it, ensure it's idempotent or handles already-tracked entities.
                    dbContext.Ratings.Add(mappedRating);
                }
            }
            // If rgEntity.Rating was null from DTO, it remains null.
            // If ReleaseGroup.RatingId is non-nullable in DB, this will cause an error at SaveChanges if rgEntity.Rating is null.

            dbContext.AttachKnownEntities(rgEntity);

            if (existingRgIdsInDb.Contains(rgEntity.Id))
            {
                dbContext.Entry(rgEntity).State = EntityState.Modified;
            }
            else
            {
                dbContext.Entry(rgEntity).State = EntityState.Added;
            }
        }

        await dbContext.SaveChangesAsync();
        MarkAsComplete();
    }

    public Task Handle(
        AddReleaseGroupToServerLibrarySagaEvents.DidNotFindReleaseGroupInMusicBrainz message
    )
    {
        MarkAsComplete();
        return Task.CompletedTask;
    }
}
