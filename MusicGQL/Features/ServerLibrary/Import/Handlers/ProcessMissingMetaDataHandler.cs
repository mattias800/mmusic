using MusicGQL.Db.Postgres;
using MusicGQL.Features.ServerLibrary.ArtistServerStatus.Services;
using Neo4j.Driver;

namespace MusicGQL.Features.ServerLibrary.Import.Handlers;

public class ProcessMissingMetaDataHandler(
    EventDbContext dbContext,
    ImportReleaseGroupToServerLibraryHandler importReleaseGroupToServerLibraryHandler,
    IDriver driver,
    ArtistServerStatusService artistServerStatusService,
    ImportArtistToServerLibraryHandler importArtistToServerLibraryHandler,
    ILogger<ProcessMissingMetaDataHandler> logger
)
{
    public async Task Handle()
    {
        logger.LogInformation("Processing missing artists in server library");

        var artistsMarked = await dbContext.ArtistsAddedToServerLibraryProjections.FindAsync(1);

        if (artistsMarked is null || artistsMarked.ArtistMbIds.Count == 0)
        {
            logger.LogInformation("No artists marked as added to server library");
            return;
        }

        var artistIdsInLibrary = new List<string>();
        await using var session = driver.AsyncSession();
        try
        {
            var reader = await session.RunAsync("MATCH (a:Artist) RETURN a.Id AS Id");
            await foreach (var record in reader)
            {
                artistIdsInLibrary.Add(record["Id"].As<string>());
            }
        }
        finally
        {
            await session.CloseAsync();
        }

        var artistIdsToAdd = artistsMarked
            .ArtistMbIds.Where(a => !artistIdsInLibrary.Contains(a))
            .ToList();

        logger.LogInformation(
            "Found {Num} artists missing in server library",
            artistIdsToAdd.Count
        );

        foreach (var artistId in artistIdsToAdd)
        {
            artistServerStatusService.SetImportingArtistStatus(artistId);
        }

        foreach (var artistId in artistIdsToAdd)
        {
            await importArtistToServerLibraryHandler.Handle(new(artistId));
        }

        await ProcessReleaseGroups();

        foreach (var artistId in artistIdsToAdd)
        {
            artistServerStatusService.SetReadyStatus(artistId);
        }
    }

    public async Task ProcessReleaseGroups()
    {
        logger.LogInformation("Processing missing release groups in server library");

        var releaseGroupsMarked =
            await dbContext.ReleaseGroupsAddedToServerLibraryProjections.FindAsync(1);

        if (releaseGroupsMarked is null || releaseGroupsMarked.ReleaseGroupMbIds.Count == 0)
        {
            logger.LogInformation("No release groups marked as added to server library");
            return;
        }

        var releaseGroupIdsInLibrary = new List<string>();
        await using var session = driver.AsyncSession();
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
