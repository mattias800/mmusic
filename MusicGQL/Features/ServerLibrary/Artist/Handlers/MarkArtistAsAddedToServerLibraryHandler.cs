using Hqub.MusicBrainz;
using MusicGQL.Db;
using MusicGQL.Db.Models.Events.ServerLibrary;

namespace MusicGQL.Features.ServerLibrary.Artist.Handlers;

public class MarkArtistAsAddedToServerLibraryHandler(
    EventDbContext dbContext,
    EventProcessor.EventProcessorWorker eventProcessorWorker,
    MusicBrainzClient client
)
{
    public async Task<Result> Handle(Command command)
    {
        var exising = await dbContext.ArtistsAddedToServerLibraryProjection.FindAsync(1);

        if (exising?.ArtistMbIds.Contains(command.ArtistId) ?? false)
        {
            return new Result.AlreadyAdded();
        }

        try
        {
            var artist = await client.Artists.GetAsync(command.ArtistId);

            if (artist is null)
            {
                return new Result.ArtistDoesNotExist();
            }
        }
        catch
        {
            return new Result.ArtistDoesNotExist();
        }

        dbContext.Events.Add(new AddArtistToServerLibrary { ArtistMbId = command.ArtistId });

        await dbContext.SaveChangesAsync();
        await eventProcessorWorker.ProcessEvents();
        return new Result.Success();
    }

    public record Command(string ArtistId);

    public abstract record Result
    {
        public record Success : Result;

        public record AlreadyAdded : Result;

        public record ArtistDoesNotExist : Result;
    }
}
