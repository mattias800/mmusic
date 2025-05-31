using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.ServerLibrary.Events;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.ServerLibrary.Artist.Handlers;

public class MarkArtistAsAddedToServerLibraryHandler(
    EventDbContext dbContext,
    EventProcessor.EventProcessorWorker eventProcessorWorker,
    MusicBrainzService mbService
)
{
    public async Task<Result> Handle(Command command)
    {
        var alreadyAdded = await dbContext.ServerArtists.AnyAsync(sa =>
            sa.ArtistId == command.ArtistId
        );

        if (alreadyAdded)
        {
            return new Result.AlreadyAdded();
        }

        try
        {
            var artist = await mbService.GetArtistByIdAsync(command.ArtistId);

            if (artist is null)
            {
                return new Result.ArtistDoesNotExist();
            }
        }
        catch
        {
            return new Result.ArtistDoesNotExist();
        }

        dbContext.Events.Add(
            new AddArtistToServerLibrary
            {
                ArtistId = command.ArtistId,
                ActorUserId = command.UserId,
            }
        );

        await dbContext.SaveChangesAsync();
        await eventProcessorWorker.ProcessEvents();
        return new Result.Success();
    }

    public record Command(Guid UserId, string ArtistId);

    public abstract record Result
    {
        public record Success : Result;

        public record AlreadyAdded : Result;

        public record ArtistDoesNotExist : Result;
    }
}
