using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.MusicBrainz.Recording;

public record MusicBrainzRecordingSearchRoot
{
    public async Task<IEnumerable<MbRecording>> SearchByName(
        [Service] MusicBrainzService mbService,
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var artists = await mbService.SearchRecordingByNameAsync(name, limit, offset);
        return artists.Select(a => new MbRecording(a));
    }

    public async Task<MbRecording?> ById([Service] MusicBrainzService mbService, [ID] string id)
    {
        var recording = await mbService.GetRecordingByIdAsync(id);
        return recording != null ? new MbRecording(recording) : null;
    }
}
