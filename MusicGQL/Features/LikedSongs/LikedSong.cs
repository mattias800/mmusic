using MusicGQL.Features.MusicBrainz.Recording;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.LikedSongs;

public record LikedSong([property: GraphQLIgnore] string RecordingId)
{
    [ID]
    public string Id => RecordingId;

    public async Task<MbRecording?> Recording([Service] MusicBrainzService mbService)
    {
        var recording = await mbService.GetRecordingByIdAsync(RecordingId);
        return recording != null ? new MbRecording(recording) : null;
    }
}
