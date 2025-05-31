using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.Likes.Commands;

public class LikeSongHandler(
    EventDbContext dbContext,
    EventProcessor.EventProcessorWorker eventProcessorWorker,
    MusicBrainzService mbService
)
{
    public async Task<Result> Handle(Command command)
    {
        var alreadyLiked = await dbContext.LikedSongs.AnyAsync(ls =>
            ls.LikedByUserId == command.UserId && ls.RecordingId == command.RecordingId
        );

        if (alreadyLiked)
        {
            return new Result.AlreadyLiked();
        }

        try
        {
            var recording = await mbService.GetRecordingByIdAsync(command.RecordingId);

            if (recording is null)
            {
                return new Result.SongDoesNotExist();
            }
        }
        catch (Exception ex)
        {
            return new Result.SongDoesNotExist();
        }

        dbContext.Events.Add(
            new Events.LikedSong
            {
                SubjectUserId = command.UserId,
                RecordingId = command.RecordingId,
            }
        );

        await dbContext.SaveChangesAsync();
        await eventProcessorWorker.ProcessEvents();
        return new Result.Success();
    }

    public record Command(Guid UserId, string RecordingId);

    public abstract record Result
    {
        public record Success : Result;

        public record AlreadyLiked : Result;

        public record SongDoesNotExist : Result;
    }
}
