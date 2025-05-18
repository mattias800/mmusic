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
            // Check if the artist is already in the database
            var dbArtist = await dbContext.Artists.FirstOrDefaultAsync(a =>
                a.Id == message.ArtistMbId
            );

            if (dbArtist == null)
            {
                logger.LogInformation("Artist not found in library database, creating new one");

                // Map and attach full object graph
                dbArtist = mapper.Map<Db.Models.ServerLibrary.MusicMetaData.Artist>(message.Artist);
                dbContext.AttachKnownEntities(dbArtist);
                dbContext.Artists.Add(dbArtist);
            }
            else
            {
                logger.LogInformation("Artist already exists in library database");
            }

            // Map incoming release groups
            var dbReleaseGroups = message
                .ReleaseGroups.Select(
                    mapper.Map<Db.Models.ServerLibrary.MusicMetaData.ReleaseGroup>
                )
                .ToList();

            // Get IDs of release groups already in the DB
            var incomingIds = dbReleaseGroups.Select(rg => rg.Id).ToList();
            var existingIds = await dbContext
                .ReleaseGroups.AsNoTracking()
                .Where(rg => incomingIds.Contains(rg.Id))
                .Select(rg => rg.Id)
                .ToHashSetAsync();

            foreach (var releaseGroup in dbReleaseGroups)
            {
                dbContext.AttachKnownEntities(releaseGroup);

                if (existingIds.Contains(releaseGroup.Id))
                {
                    // Mark for update
                    dbContext.Entry(releaseGroup).State = EntityState.Modified;
                }
                else
                {
                    // Insert new
                    dbContext.ReleaseGroups.Add(releaseGroup);
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
