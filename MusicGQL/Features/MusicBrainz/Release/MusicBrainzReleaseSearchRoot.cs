using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.MusicBrainz.Release;

public record MusicBrainzReleaseSearchRoot
{
    public async Task<IEnumerable<Release>> SearchByName(
        [Service] MusicBrainzService mbService,
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var releases = await mbService.SearchReleaseByNameAsync(name, limit, offset);
        return releases.Select(a => new Release(a));
    }

    public async Task<Release?> ById([Service] MusicBrainzService mbService, [ID] string id)
    {
        try
        {
            var recording = await mbService.GetReleaseByIdAsync(id);
            return recording != null ? new Release(recording) : null;
        }
        catch
        {
            return null;
        }
    }
}
