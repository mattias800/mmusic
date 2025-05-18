using Microsoft.EntityFrameworkCore;
using MusicGQL.Db;
using MusicGQL.Features.ServerLibrary.Artist.Sagas;
using Rebus.Bus;

namespace MusicGQL.Features.ServerLibrary.Artist.Handlers;

public class ProcessMissingArtistsInServerLibraryHandler(
    EventDbContext dbContext,
    IBus bus,
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

        var artistIdsInLibrary = await dbContext.Artists.Select(a => a.Id).ToListAsync();

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
