using Hqub.MusicBrainz;
using MusicGQL.Aggregates;
using MusicGQL.Db;

namespace MusicGQL.Features.LikedSongs.Commands;

public class LikeSongHandler(
    EventDbContext dbContext,
    EventProcessor eventProcessor,
    MusicBrainzClient client
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
            var recording = await client.Recordings.GetAsync(command.RecordingId);

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
            new MusicGQL.Db.Models.Events.LikedSong { RecordingId = command.RecordingId }
        );

        await dbContext.SaveChangesAsync();
        await eventProcessor.ProcessEvents();
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
