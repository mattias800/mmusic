using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models;
using MusicGQL.Db.Postgres.Models.Events.ServerLibrary;
using MusicGQL.Db.Postgres.Models.Projections;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup.Aggregate;

public class ReleaseGroupsAddedToServerLibraryProcessor
    : EventProcessor.EventProcessor<ReleaseGroupsAddedToServerLibraryProjection>
{
    private ReleaseGroupsAddedToServerLibraryProjection? _projection;

    public override async Task PrepareProcessing(EventDbContext dbContext)
    {
        _projection = await dbContext.ReleaseGroupsAddedToServerLibraryProjection.FindAsync(1);

        if (_projection == null)
        {
            _projection = new ReleaseGroupsAddedToServerLibraryProjection { Id = 1 };
            dbContext.ReleaseGroupsAddedToServerLibraryProjection.Add(_projection);
        }
    }

    public override ReleaseGroupsAddedToServerLibraryProjection? GetAggregate()
    {
        return _projection;
    }

    protected override void ProcessEvent(
        Event ev,
        EventDbContext dbContext,
        ReleaseGroupsAddedToServerLibraryProjection aggregate
    )
    {
        switch (ev)
        {
            case AddReleaseGroupToServerLibrary e:
                if (!aggregate.ReleaseGroupMbIds.Contains(e.ReleaseGroupMbId))
                {
                    aggregate.ReleaseGroupMbIds.Add(e.ReleaseGroupMbId);
                    aggregate.LastUpdatedAt = DateTime.UtcNow;
                }

                break;
        }
    }
}
