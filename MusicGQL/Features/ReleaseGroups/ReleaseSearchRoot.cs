using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.ReleaseGroups;

public record ReleaseGroupSearchRoot
{
    public async Task<IEnumerable<ReleaseGroup>> SearchByName(
        [Service] MusicBrainzService mbService,
        string name
    )
    {
        var releases = await mbService.SearchReleaseGroupByNameAsync(name);
        return releases.Select(a => new ReleaseGroup(a));
    }

    public async Task<ReleaseGroup?> ById([Service] MusicBrainzService mbService, [ID] string id)
    {
        try
        {
            var recording = await mbService.GetReleaseGroupByIdAsync(id);
            return recording != null ? new ReleaseGroup(recording) : null;
        }
        catch
        {
            return null;
        }
    }
}