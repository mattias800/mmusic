using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Likes.Events;

namespace MusicGQL.Features.Likes.Commands;

public class UnlikeSongHandler(
    EventDbContext dbContext,
    EventProcessor.EventProcessorWorker eventProcessorWorker
)
{
    public async Task<Result> Handle(Command command)
    {
        var isLiked = await dbContext.LikedSongs.AnyAsync(ls =>
            ls.LikedByUserId == command.UserId && ls.RecordingId == command.RecordingId
        );

        if (!isLiked)
        {
            return new Result.AlreadyNotLiked();
        }

        dbContext.Events.Add(
            new UnlikedSong { SubjectUserId = command.UserId, RecordingId = command.RecordingId }
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
