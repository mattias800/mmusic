using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models;
using MusicGQL.Features.ServerLibrary.Artist.Db;
using MusicGQL.Features.ServerLibrary.Events;

namespace MusicGQL.Features.ServerLibrary.Artist.Aggregate;

public class ArtistsAddedToServerLibraryProcessor
    : EventProcessor.EventProcessor<ArtistsAddedToServerLibraryProjection>
{
    ArtistsAddedToServerLibraryProjection? _projection;

    public override async Task PrepareProcessing(EventDbContext dbContext)
    {
        _projection = await dbContext.ArtistsAddedToServerLibraryProjections.FindAsync(1);

        if (_projection == null)
        {
            _projection = new ArtistsAddedToServerLibraryProjection { Id = 1 };
            dbContext.ArtistsAddedToServerLibraryProjections.Add(_projection);
        }
    }

    public override ArtistsAddedToServerLibraryProjection? GetAggregate() => _projection;

    protected override void ProcessEvent(
        Event ev,
        EventDbContext dbContext,
        ArtistsAddedToServerLibraryProjection aggregate
    )
    {
        switch (ev)
        {
            case AddArtistToServerLibrary e:
                if (!aggregate.ArtistMbIds.Contains(e.ArtistMbId))
                {
                    aggregate.ArtistMbIds.Add(e.ArtistMbId);
                    aggregate.LastUpdatedAt = DateTime.UtcNow;
                }

                break;
        }
    }
}
