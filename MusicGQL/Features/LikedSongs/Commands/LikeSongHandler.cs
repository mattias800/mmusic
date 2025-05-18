using MusicGQL.Db;
using MusicGQL.Db.Postgres;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.LikedSongs.Commands;

public class LikeSongHandler(
    EventDbContext dbContext,
    EventProcessor.EventProcessorWorker eventProcessorWorker,
    MusicBrainzService mbService
)
{
    public async Task<Result> Handle(Command command)
    {
        var exising = await dbContext.LikedSongsProjections.FindAsync(1);

        if (exising?.LikedSongRecordingIds.Contains(command.RecordingId) ?? false)
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
        catch
        {
            return new Result.SongDoesNotExist();
        }

        dbContext.Events.Add(
            new Db.Postgres.Models.Events.LikedSong { RecordingId = command.RecordingId }
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
