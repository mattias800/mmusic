using MusicGQL.Db.Postgres;

namespace MusicGQL.Features.ServerSettings.Commands;

public class UpdatePublicBaseUrlHandler(
    EventDbContext dbContext,
    EventProcessor.EventProcessorWorker eventProcessorWorker
)
{
    public async Task<Result> Handle(Command command)
    {
        dbContext.Events.Add(
            new Events.PublicBaseUrlUpdated
            {
                ActorUserId = command.UserId,
                NewPublicBaseUrl = command.NewPublicBaseUrl,
            }
        );

        await dbContext.SaveChangesAsync();
        await eventProcessorWorker.ProcessEvents();
        return new Result.Success();
    }

    public record Command(Guid UserId, string NewPublicBaseUrl);

    public abstract record Result
    {
        public record Success : Result;
    }
}
