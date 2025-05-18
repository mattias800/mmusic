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
        logger.LogInformation("Saving artist and release groups to library database");

        try
        {
            // --- Artist Handling ---
            var artistEntity = await dbContext.Artists.FirstOrDefaultAsync(a => a.Id == message.ArtistMbId);

            if (artistEntity == null)
            {
                logger.LogInformation("Artist not found in library database, creating new one");
                artistEntity = mapper.Map<Db.Models.ServerLibrary.MusicMetaData.Artist>(message.Artist);
                dbContext.Artists.Add(artistEntity); // EF Core starts tracking artistEntity
            }
            else
            {
                logger.LogInformation("Artist already exists in library database, updating existing one");
                mapper.Map(message.Artist, artistEntity); // Update properties of tracked artistEntity
            }

            // --- Release Group Handling ---
            var allIncomingRgIds = message.ReleaseGroups.Select(rg => rg.Id).ToHashSet();
            var existingRgIdsInDb = await dbContext.ReleaseGroups
                .AsNoTracking()
                .Where(rg => allIncomingRgIds.Contains(rg.Id))
                .Select(rg => rg.Id)
                .ToHashSetAsync();

            foreach (var rgDto in message.ReleaseGroups)
            {
                var rgEntity = mapper.Map<Db.Models.ServerLibrary.MusicMetaData.ReleaseGroup>(rgDto);

                // ** CORRECTED FIX for Artist tracking conflict via NameCredit **
                if (rgEntity.Credits != null)
                {
                    foreach (var nameCredit in rgEntity.Credits)
                    {
                        // If this NameCredit's artist is the main artist we are processing,
                        // ensure it points to the single, tracked artistEntity instance.
                        if (nameCredit.Artist != null && nameCredit.Artist.Id == artistEntity.Id)
                        {
                            nameCredit.Artist = artistEntity;
                        }
                        // If nameCredit.Artist is some *other* artist not yet tracked or a different one,
                        // AttachKnownEntities will handle it. The critical part is not to re-track artistEntity.
                    }
                }

                // AttachKnownEntities will process rgEntity and its children (e.g., rgEntity.Area, rgEntity.Credits).
                // Artists within Credits should now either be the correctly tracked artistEntity or other artists to be processed by AttachKnownEntities.
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
