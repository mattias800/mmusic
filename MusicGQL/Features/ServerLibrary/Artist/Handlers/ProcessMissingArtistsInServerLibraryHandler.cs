using Microsoft.EntityFrameworkCore;
using MusicGQL.Db;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.ServerLibrary.Artist.Sagas;
using Neo4j.Driver;
using Rebus.Bus;

namespace MusicGQL.Features.ServerLibrary.Artist.Handlers;

public class ProcessMissingArtistsInServerLibraryHandler(
    EventDbContext dbContext,
    IBus bus,
    IDriver neo4jDriver,
    ILogger<ProcessMissingArtistsInServerLibraryHandler> logger
)
{
    public async Task<Result> Handle(Command command)
    {
        logger.LogInformation("Processing missing artists in server library");

        var artistsMarked = await dbContext.ArtistsAddedToServerLibraryProjection.FindAsync(1);

        if (artistsMarked is null || artistsMarked.ArtistMbIds.Count == 0)
        {
            logger.LogInformation("No artists marked as added to server library");
            return new Result.Success();
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
            logger.LogInformation("Adding artist {Id} to server library", artistId);
            await bus.Send(new AddArtistToServerLibrarySagaEvents.StartAddArtist(artistId));
        }

        return new Result.Success();
    }

    public record Command;

    public abstract record Result
    {
        public record Success : Result;

        public record AlreadyAdded : Result;

        public record ArtistDoesNotExist : Result;
    }
}
