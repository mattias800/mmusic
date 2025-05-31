using MusicGQL.Db.Postgres;
using MusicGQL.Features.Playlists.Events;

namespace MusicGQL.Features.Playlists.Commands;

public class RenamePlaylistHandler(
    EventDbContext dbContext,
    EventProcessor.EventProcessorWorker eventProcessorWorker,
    VerifyPlaylistWriteAccessHandler verifyPlaylistWriteAccessHandler
)
{
    public async Task<Result> Handle(Command command)
    {
        var writeAccess = await verifyPlaylistWriteAccessHandler.VerifyPlaylistWriteAccess(
            new VerifyPlaylistWriteAccessHandler.Query(command.UserId, command.PlaylistId)
        );

        return writeAccess switch
        {
            VerifyPlaylistWriteAccessHandler.Result.WritesNotAllowed => new Result.NotAllowed(),
            VerifyPlaylistWriteAccessHandler.Result.WritesAllowed => await DoIt(command),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private async Task<Result> DoIt(Command command)
    {
        dbContext.Events.Add(
            new RenamedPlaylist
            {
                ActorUserId = command.UserId,
                PlaylistId = command.PlaylistId,
                NewPlaylistName = command.NewPlaylistName,
            }
        );

        await dbContext.SaveChangesAsync();
        await eventProcessorWorker.ProcessEvents();
        return new Result.Success();
    }

    public record Command(Guid UserId, Guid PlaylistId, string NewPlaylistName);

    public abstract record Result
    {
        public record Success : Result;

        public record NotAllowed : Result;
    }
}
