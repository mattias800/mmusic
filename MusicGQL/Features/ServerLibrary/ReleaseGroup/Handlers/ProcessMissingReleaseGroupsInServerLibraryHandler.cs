using Microsoft.EntityFrameworkCore;
using MusicGQL.Db;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Sagas;
using Rebus.Bus;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup.Handlers;

public class ProcessMissingReleaseGroupsInServerLibraryHandler(
    EventDbContext dbContext,
    IBus bus,
    ILogger<ProcessMissingReleaseGroupsInServerLibraryHandler> logger
)
{
    public async Task<Result> Handle(Command command)
    {
        logger.LogInformation("Processing missing release groups in server library");

        var releaseGroupsMarked =
            await dbContext.ReleaseGroupsAddedToServerLibraryProjection.FindAsync(1);

        if (releaseGroupsMarked is null || releaseGroupsMarked.ReleaseGroupMbIds.Count == 0)
        {
            logger.LogInformation("No release groups marked as added to server library");
            return new Result.Success();
        }

        var releaseGroupIdsInLibrary = await dbContext
            .ReleaseGroups.Select(a => a.Id)
            .ToListAsync();

        var releaseGroupIdsToAdd = releaseGroupsMarked
            .ReleaseGroupMbIds.Where(a => !releaseGroupIdsInLibrary.Contains(a))
            .ToList();

        logger.LogInformation(
            "Found {Num} release groups missing in server library",
            releaseGroupIdsToAdd.Count
        );

        foreach (var releaseGroupId in releaseGroupIdsToAdd)
        {
            logger.LogInformation("Adding release group {Id} to server library", releaseGroupId);
            await bus.Send(
                new AddReleaseGroupToServerLibrarySagaEvents.StartAddReleaseGroup(releaseGroupId)
            );
        }

        return new Result.Success();
    }

    public record Command;

    public abstract record Result
    {
        public record Success : Result;

        public record AlreadyAdded : Result;

        public record ReleaseGroupDoesNotExist : Result;
    }
}
