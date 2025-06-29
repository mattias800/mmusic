using MusicGQL.Db.Postgres;

namespace MusicGQL.Features.ServerSettings.Commands;

public class UpdateLibraryPathHandler(
    EventDbContext dbContext,
    EventProcessor.EventProcessorWorker eventProcessorWorker
)
{
    public async Task<Result> Handle(Command command)
    {
        dbContext.Events.Add(
            new Events.LibraryPathUpdated
            {
                ActorUserId = command.UserId,
                NewLibraryPath = command.NewLibraryPath,
            }
        );

        await dbContext.SaveChangesAsync();
        await eventProcessorWorker.ProcessEvents();
        return new Result.Success();
    }

    public record Command(Guid UserId, string NewLibraryPath);

    public abstract record Result
    {
        public record Success : Result;
    }
}
