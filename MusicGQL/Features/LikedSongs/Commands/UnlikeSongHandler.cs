using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models.Events;

namespace MusicGQL.Features.LikedSongs.Commands;

public class UnlikeSongHandler(
    EventDbContext dbContext,
    MusicGQL.EventProcessor.EventProcessorWorker eventProcessorWorker
)
{
    public async Task<Result> Handle(Command command)
    {
        var existingProjection = await dbContext.LikedSongsProjections.FirstOrDefaultAsync(p =>
            p.UserId == command.UserId
        );

        if (!(existingProjection?.LikedSongRecordingIds.Contains(command.RecordingId) ?? false))
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
