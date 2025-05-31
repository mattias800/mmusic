using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models.Events.Playlists;

namespace MusicGQL.Features.Playlists.Commands;

public class CreatePlaylistHandler(
    EventDbContext dbContext,
    EventProcessor.EventProcessorWorker eventProcessorWorker
)
{
    public async Task<Result> Handle(Command command)
    {
        dbContext.Events.Add(
            new CreatedPlaylist
            {
                PlaylistId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                ActorUserId = command.UserId,
            }
        );

        await dbContext.SaveChangesAsync();
        await eventProcessorWorker.ProcessEvents();
        return new Result.Success();
    }

    public record Command(Guid UserId);

    public abstract record Result
    {
        public record Success : Result;
    }
}
