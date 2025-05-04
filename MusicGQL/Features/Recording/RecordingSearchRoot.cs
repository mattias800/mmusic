using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.Recording;

public record RecordingSearchRoot
{
    public async Task<IEnumerable<Recording>> SearchByName(
        [Service] MusicBrainzService mbService,
        string name
    )
    {
        var artists = await mbService.SearchRecordingByNameAsync(name);
        return artists.Select(a => new Recording(a));
    }

    public async Task<Recording?> ById([Service] MusicBrainzService mbService, [ID] string id)
    {
        var recording = await mbService.GetRecordingByIdAsync(id);
        return recording != null ? new Recording(recording) : null;
    }
}
