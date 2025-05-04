using MbRelease = Hqub.MusicBrainz.Entities.Release;

namespace MusicGQL.Features.Release;

public static class MainAlbumFinder
{
    public static MbRelease? GetMainReleaseInReleaseGroup(List<MbRelease> releases)
    {
        var official = releases.Where(r => r.Status == "Official").ToList();

        var releasesAvailable = official.Count != 0 ? official : releases;

        return releasesAvailable // filter by official only
            .OrderBy(r =>
                r.Country switch
                {
                    "XW" => 0,
                    "US" => 1,
                    "GB" => 2,
                    _ => 3
                }
            )
            .ThenBy(r => r.Date) // prefer earlier date
            .LastOrDefault() ?? releases.LastOrDefault();
    }

    public static MbRelease? FindMainAlbumForSong(List<MbRelease> releases)
    {
        var official = releases.Where(r => r.Status == "Official").ToList();

        var releasesAvailable = official.Count != 0 ? official : releases;

        var allAlbums = releasesAvailable
            .Where(r => r.ReleaseGroup?.PrimaryType == "Album" && r.ReleaseGroup.SecondaryTypes.Count == 0).ToList();

        if (allAlbums.Count > 0)
        {
            return FindPrioritizedRegionalAlbum(allAlbums);
        }

        var allEps = releasesAvailable.Where(r => r.ReleaseGroup?.PrimaryType == "EP").ToList();

        if (allEps.Count > 0)
        {
            return FindPrioritizedRegionalAlbum(allEps);
        }

        var allSingles = releasesAvailable.Where(r => r.ReleaseGroup?.PrimaryType == "Single").ToList();

        if (allSingles.Count > 0)
        {
            return FindPrioritizedRegionalAlbum(allSingles);
        }

        return releasesAvailable.First();
    }

    public static MbRelease? FindPrioritizedRegionalAlbum(List<MbRelease> releases)
    {
        if (releases.Count == 1)
        {
            return releases.First();
        }

        return releases.LastOrDefault(a => a.Country == "XW")
               ?? releases.LastOrDefault(a => a.Country == "US")
               ?? releases.LastOrDefault(a => a.Country == "GB")
               ?? releases.LastOrDefault();
    }
};