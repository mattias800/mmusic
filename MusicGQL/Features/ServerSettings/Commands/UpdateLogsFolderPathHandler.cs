using MusicGQL.Db.Postgres;

namespace MusicGQL.Features.ServerSettings.Commands;

public class UpdateLogsFolderPathHandler(
    EventDbContext dbContext,
    EventProcessor.EventProcessorWorker eventProcessorWorker
)
{
    public async Task<Result> Handle(Command command)
    {
        dbContext.Events.Add(
            new Events.LogsFolderPathUpdated
            {
                ActorUserId = command.UserId,
                NewPath = command.NewPath,
            }
        );

        await dbContext.SaveChangesAsync();
        await eventProcessorWorker.ProcessEvents();
        return new Result.Success();
    }

    public record Command(Guid UserId, string? NewPath);

    public abstract record Result
    {
        public record Success : Result;
    }
}
