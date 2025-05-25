using MusicGQL.Db.Postgres;
using Neo4j.Driver;

namespace MusicGQL.Features.ServerLibrary.Artist.Handlers;

public class ProcessMissingArtistsInServerLibraryHandler(
    EventDbContext dbContext,
    IDriver neo4jDriver,
    ImportArtistToServerLibraryHandler importArtistToServerLibraryHandler,
    ILogger<ProcessMissingArtistsInServerLibraryHandler> logger
)
{
    public async Task Handle()
    {
        logger.LogInformation("Processing missing artists in server library");

        var artistsMarked = await dbContext.ArtistsAddedToServerLibraryProjection.FindAsync(1);

        if (artistsMarked is null || artistsMarked.ArtistMbIds.Count == 0)
        {
            logger.LogInformation("No artists marked as added to server library");
            return;
        }

        var artistIdsInLibrary = new List<string>();
        await using var session = neo4jDriver.AsyncSession();
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
            await importArtistToServerLibraryHandler.Handle(new(artistId));
        }
    }
}
