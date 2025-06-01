using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.ServerLibrary.ArtistServerStatus.Services;
using MusicGQL.Integration.MusicBrainz;
using Neo4j.Driver;

namespace MusicGQL.Features.ServerLibrary.Import.Handlers;

public class ProcessMissingMetaDataHandler(
    EventDbContext dbContext,
    ImportReleaseGroupToServerLibraryHandler importReleaseGroupToServerLibraryHandler,
    IDriver driver,
    ArtistServerStatusService artistServerStatusService,
    ImportArtistToServerLibraryHandler importArtistToServerLibraryHandler,
    ImportArtistReleaseGroupsToServerLibraryHandler importArtistReleaseGroupsToServerLibraryHandler,
    ILogger<ProcessMissingMetaDataHandler> logger
)
{
    public async Task ProcessMissingArtists()
    {
        logger.LogInformation("Processing missing artists in server library");

        var artistsMarkedMbIds = await dbContext
            .ServerArtists.Select(sa => sa.ArtistId)
            .Distinct()
            .ToListAsync();

        if (!artistsMarkedMbIds.Any())
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

        var artistIdsToAdd = artistsMarkedMbIds
            .Where(a => !artistIdsInLibrary.Contains(a))
            .ToList();

        logger.LogInformation(
            "Found {Count} artists missing in server library",
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

        foreach (var artistId in artistIdsToAdd)
        {
            await importArtistReleaseGroupsToServerLibraryHandler.Handle(new(artistId));
        }

        foreach (var artistId in artistIdsToAdd)
        {
            artistServerStatusService.SetReadyStatus(artistId);
        }
    }

    public async Task ProcessMissingReleaseGroups()
    {
        logger.LogInformation("Processing missing release groups in server library");

        var releaseGroupsMarkedMbIds = await dbContext
            .ServerReleaseGroups.Select(srg => srg.ReleaseGroupId)
            .Distinct()
            .ToListAsync();

        if (!releaseGroupsMarkedMbIds.Any())
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

        var releaseGroupIdsToAdd = releaseGroupsMarkedMbIds
            .Where(id => !releaseGroupIdsInLibrary.Contains(id))
            .ToList();

        logger.LogInformation(
            "Found {Count} release groups missing in server library",
            releaseGroupIdsToAdd.Count
        );

        await Task.WhenAll(
            releaseGroupIdsToAdd.Select(async releaseGroupId =>
                await importReleaseGroupToServerLibraryHandler.Handle(new(releaseGroupId))
            )
        );
    }
}
