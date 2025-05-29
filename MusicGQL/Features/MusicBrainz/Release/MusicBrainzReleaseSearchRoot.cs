using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.MusicBrainz.Release;

public record MusicBrainzReleaseSearchRoot
{
    public async Task<IEnumerable<MbRelease>> SearchByName(
        [Service] MusicBrainzService mbService,
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var releases = await mbService.SearchReleaseByNameAsync(name, limit, offset);
        return releases.Select(a => new MbRelease(a));
    }

    public async Task<MbRelease?> ById([Service] MusicBrainzService mbService, [ID] string id)
    {
        try
        {
            var recording = await mbService.GetReleaseByIdAsync(id);
            return recording != null ? new MbRelease(recording) : null;
        }
        catch
        {
            return null;
        }
    }
}
