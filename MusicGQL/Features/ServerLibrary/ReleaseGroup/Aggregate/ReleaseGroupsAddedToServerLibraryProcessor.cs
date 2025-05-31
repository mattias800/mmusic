using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models;
using MusicGQL.Features.ServerLibrary.Events;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Db;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup.Aggregate;

public class ReleaseGroupsAddedToServerLibraryProcessor(
    ILogger<ReleaseGroupsAddedToServerLibraryProcessor> logger
)
{
    public async Task ProcessEvent(Event ev, EventDbContext dbContext)
    {
        switch (ev)
        {
            case AddReleaseGroupToServerLibrary { ActorUserId: null } addEvent:
                logger.LogWarning(
                    "AddReleaseGroupToServerLibrary event is missing ActorUserId. ReleaseGroupMbId: {ReleaseGroupMbId}",
                    addEvent.ReleaseGroupId
                );
                return;

            case AddReleaseGroupToServerLibrary addEvent:
            {
                var existing = dbContext.ServerReleaseGroups.FirstOrDefault(srg =>
                    srg.ReleaseGroupId == addEvent.ReleaseGroupId
                );

                if (existing == null)
                {
                    var newServerReleaseGroup = new DbServerReleaseGroup
                    {
                        AddedByUserId = addEvent.ActorUserId.Value,
                        ReleaseGroupId = addEvent.ReleaseGroupId,
                        AddedAt = addEvent.CreatedAt,
                    };
                    dbContext.ServerReleaseGroups.Add(newServerReleaseGroup);
                    logger.LogInformation(
                        "ReleaseGroupMbId {ReleaseGroupMbId} added to server library by UserId: {UserId}",
                        addEvent.ReleaseGroupId,
                        addEvent.ActorUserId.Value
                    );
                    await dbContext.SaveChangesAsync();
                }

                break;
            }
        }
    }
}
