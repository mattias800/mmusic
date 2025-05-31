using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models;
using MusicGQL.Features.ServerLibrary.Artist.Db;
using MusicGQL.Features.ServerLibrary.Events;

namespace MusicGQL.Features.ServerLibrary.Artist.Aggregate;

public class ArtistsAddedToServerLibraryProcessor(
    ILogger<ArtistsAddedToServerLibraryProcessor> logger
)
{
    public async Task ProcessEvent(Event ev, EventDbContext dbContext)
    {
        switch (ev)
        {
            // Ensure UserId is present in the event
            case AddArtistToServerLibrary { ActorUserId: null } addEvent:
                logger.LogWarning(
                    "AddArtistToServerLibrary event is missing ActorUserId. ArtistMbId: {ArtistMbId}",
                    addEvent.ArtistId
                );
                return;

            case AddArtistToServerLibrary addEvent:
            {
                var existing = dbContext.ServerArtists.FirstOrDefault(sa =>
                    sa.AddedByUserId == addEvent.ActorUserId.Value
                    && sa.ArtistId == addEvent.ArtistId
                );

                if (existing == null)
                {
                    var newServerArtist = new DbServerArtist
                    {
                        AddedByUserId = addEvent.ActorUserId.Value,
                        ArtistId = addEvent.ArtistId,
                        AddedAt = addEvent.CreatedAt,
                    };
                    dbContext.ServerArtists.Add(newServerArtist);
                    logger.LogInformation(
                        "ArtistMbId {ArtistMbId} added to server library by UserId: {UserId}",
                        addEvent.ArtistId,
                        addEvent.ActorUserId.Value
                    );
                    await dbContext.SaveChangesAsync();
                }

                return;
            }
        }
    }
}
