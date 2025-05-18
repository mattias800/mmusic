using MusicGQL.Db;
using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models.Events.ServerLibrary;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup.Handlers;

public class MarkReleaseGroupAsAddedToServerLibraryHandler(
    EventDbContext dbContext,
    EventProcessor.EventProcessorWorker eventProcessorWorker,
    MusicBrainzService mbService
)
{
    public async Task<Result> Handle(Command command)
    {
        var exising = await dbContext.ReleaseGroupsAddedToServerLibraryProjection.FindAsync(1);

        if (exising?.ReleaseGroupMbIds.Contains(command.ReleaseGroupId) ?? false)
        {
            return new Result.AlreadyAdded();
        }

        try
        {
            var artist = await mbService.GetReleaseGroupByIdAsync(command.ReleaseGroupId);

            if (artist is null)
            {
                return new Result.ReleaseGroupDoesNotExist();
            }
        }
        catch
        {
            return new Result.ReleaseGroupDoesNotExist();
        }

        dbContext.Events.Add(
            new AddReleaseGroupToServerLibrary { ReleaseGroupMbId = command.ReleaseGroupId }
        );

        await dbContext.SaveChangesAsync();
        await eventProcessorWorker.ProcessEvents();
        return new Result.Success();
    }

    public record Command(string ReleaseGroupId);

    public abstract record Result
    {
        public record Success : Result;

        public record AlreadyAdded : Result;

        public record ReleaseGroupDoesNotExist : Result;
    }
}
