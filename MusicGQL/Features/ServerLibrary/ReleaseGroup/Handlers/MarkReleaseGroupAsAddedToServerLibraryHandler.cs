using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.ServerLibrary.Events;
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
        var alreadyAdded = await dbContext.ServerReleaseGroups.AnyAsync(srg =>
            srg.AddedByUserId == command.UserId && srg.ReleaseGroupId == command.ReleaseGroupId
        );

        if (alreadyAdded)
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
            new AddReleaseGroupToServerLibrary
            {
                ActorUserId = command.UserId,
                ReleaseGroupId = command.ReleaseGroupId,
            }
        );

        await dbContext.SaveChangesAsync();
        await eventProcessorWorker.ProcessEvents();
        return new Result.Success();
    }

    public record Command(Guid UserId, string ReleaseGroupId);

    public abstract record Result
    {
        public record Success : Result;

        public record AlreadyAdded : Result;

        public record ReleaseGroupDoesNotExist : Result;
    }
}
