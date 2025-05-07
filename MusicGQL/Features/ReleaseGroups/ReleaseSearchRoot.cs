using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.ReleaseGroups;

public record ReleaseGroupSearchRoot
{
    [ID]
    public string GetId() => "ReleaseGroupSearchRoot";

    public async Task<IEnumerable<ReleaseGroup>> SearchByName(
        [Service] MusicBrainzService mbService,
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var releases = await mbService.SearchReleaseGroupByNameAsync(name, limit, offset);
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
