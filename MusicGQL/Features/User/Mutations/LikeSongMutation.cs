using MusicGQL.Aggregates;
using MusicGQL.Db;
using MusicGQL.Db.Models.Events;
using MusicGQL.Types;

namespace MusicGQL.Features.User.Mutations;

[ExtendObjectType(typeof(Mutation))]
public record LikeSongMutation
{
    public async Task<LikedSongPayload> LikeSong(EventDbContext dbContext, [Service] EventProcessor eventProcessor,
        string recordingId)
    {
        dbContext.Events.Add(new LikedSong
        {
            RecordingId = recordingId
        });

        await dbContext.SaveChangesAsync();
        await eventProcessor.ProcessEvents();
        return new(true);
    }
}

public record LikedSongPayload(bool Success);