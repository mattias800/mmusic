using Hqub.MusicBrainz;

namespace MusicGQL.Features.LikedSongs;

public record LikedSong([property: GraphQLIgnore] string RecordingId)
{
    [ID] public string Id => RecordingId;

    public async Task<Recording.Recording?> Recording([Service] MusicBrainzClient client)
    {
        var recording = await client.Recordings.GetAsync(RecordingId);
        return recording != null ? new Recording.Recording(recording) : null;
    }
}