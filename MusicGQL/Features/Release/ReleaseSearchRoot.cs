using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.Release;

public record ReleaseSearchRoot
{
    public async Task<IEnumerable<Release>> SearchByName(
        [Service] MusicBrainzService mbService,
        string name
    )
    {
        var releases = await mbService.SearchReleaseByNameAsync(name);
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
