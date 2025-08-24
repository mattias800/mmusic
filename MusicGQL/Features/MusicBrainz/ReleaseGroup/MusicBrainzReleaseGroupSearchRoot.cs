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

    public async Task<IEnumerable<MbReleaseGroup>> SearchByNameAndArtistName(
        [Service] MusicBrainzService mbService,
        string name,
        string artistName,
        int limit = 25,
        int offset = 0
    )
    {
        // MusicBrainz doesn't provide a combined search in the wrapper used here.
        // Strategy: search by name, then filter to those whose primary credited artist matches artistName (case-insensitive contains/equals).
        var groups = await mbService.SearchReleaseGroupByNameAsync(name, limit, offset);
        if (string.IsNullOrWhiteSpace(artistName))
            return groups.Select(g => new MbReleaseGroup(g));

        var filtered = groups.Where(g =>
        {
            var credits = g.Credits ?? new List<Hqub.MusicBrainz.Entities.NameCredit>();
            var primary = credits.FirstOrDefault()?.Artist?.Name ?? credits.FirstOrDefault()?.Name;
            if (string.IsNullOrWhiteSpace(primary))
                return false;
            return primary!.Equals(artistName, StringComparison.OrdinalIgnoreCase)
                || primary!.Contains(artistName, StringComparison.OrdinalIgnoreCase);
        });

        return filtered.Select(g => new MbReleaseGroup(g));
    }
};
