using MusicGQL.Db;

namespace MusicGQL.Features.LikedSongs.Commands;

public class UnlikeSongHandler(
    EventDbContext dbContext,
    EventProcessor.EventProcessorWorker eventProcessorWorker
)
{
    public async Task<Result> Handle(Command command)
    {
        var exising = await dbContext.LikedSongsProjections.FindAsync(1);

        if (!(exising?.LikedSongRecordingIds.Contains(command.RecordingId) ?? false))
        {
            return new Result.AlreadyNotLiked();
        }

        dbContext.Events.Add(
            new Db.Models.Events.UnlikedSong { RecordingId = command.RecordingId }
        );

        await dbContext.SaveChangesAsync();
        await eventProcessorWorker.ProcessEvents();
        return new Result.Success();
    }

    public record Command(Guid UserId, string RecordingId);

    public abstract record Result
    {
        public record Success : Result;

        public record AlreadyNotLiked : Result;
    }
}
