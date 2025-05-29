using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.MusicBrainz.ReleaseGroup;

public record MusicBrainzReleaseGroupSearchRoot
{
    public async Task<IEnumerable<MbReleaseGroup>> SearchByName(
        [Service] MusicBrainzService mbService,
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var releases = await mbService.SearchReleaseGroupByNameAsync(name, limit, offset);
        return releases.Select(a => new MbReleaseGroup(a));
    }

    public async Task<MbReleaseGroup?> ById([Service] MusicBrainzService mbService, [ID] string id)
    {
        try
        {
            var recording = await mbService.GetReleaseGroupByIdAsync(id);
            return recording != null ? new MbReleaseGroup(recording) : null;
        }
        catch
        {
            return null;
        }
    }
};
