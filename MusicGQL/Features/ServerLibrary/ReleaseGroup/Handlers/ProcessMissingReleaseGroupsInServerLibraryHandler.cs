using MusicGQL.Db.Postgres;
using Neo4j.Driver;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup.Handlers;

public class ProcessMissingReleaseGroupsInServerLibraryHandler(
    EventDbContext dbContext,
    IDriver neo4jDriver,
    ImportReleaseGroupToServerLibraryHandler importReleaseGroupToServerLibraryHandler,
    ILogger<ProcessMissingReleaseGroupsInServerLibraryHandler> logger
)
{
    public async Task Handle()
    {
        logger.LogInformation("Processing missing release groups in server library");

        var releaseGroupsMarked =
            await dbContext.ReleaseGroupsAddedToServerLibraryProjection.FindAsync(1);

        if (releaseGroupsMarked is null || releaseGroupsMarked.ReleaseGroupMbIds.Count == 0)
        {
            logger.LogInformation("No release groups marked as added to server library");
            return;
        }

        var releaseGroupIdsInLibrary = new List<string>();
        await using var session = neo4jDriver.AsyncSession();
        try
        {
            var reader = await session.RunAsync("MATCH (rg:ReleaseGroup) RETURN rg.Id AS Id");
            await foreach (var record in reader)
            {
                releaseGroupIdsInLibrary.Add(record["Id"].As<string>());
            }
        }
        finally
        {
            await session.CloseAsync();
        }

        var releaseGroupIdsToAdd = releaseGroupsMarked
            .ReleaseGroupMbIds.Where(id => !releaseGroupIdsInLibrary.Contains(id))
            .ToList();

        logger.LogInformation(
            "Found {Num} release groups missing in server library",
            releaseGroupIdsToAdd.Count
        );

        await Task.WhenAll(
            releaseGroupIdsToAdd.Select(async releaseGroupId =>
                await importReleaseGroupToServerLibraryHandler.Handle(new(releaseGroupId))
            )
        );
    }
}
