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
                r.Country == "XW" ? 0 :
                r.Country == "US" ? 1 :
                r.Country == "GB" ? 2 : 3
            )
            .ThenBy(r => r.Date) // prefer earlier date
            .FirstOrDefault() ?? releasesAvailable.FirstOrDefault();
    }

    public static MbRelease FindMainAlbumForSong(
        List<MbRelease> releaseList)
    {
        var allAlbums = releaseList
            .Where(r => r.ReleaseGroup?.PrimaryType == "Album").ToList();

        if (allAlbums.Count > 0)
        {
            return FindPrioritizedRegionalAlbum(allAlbums);
        }

        var allEps = releaseList
            .Where(r => r.ReleaseGroup?.PrimaryType == "EP").ToList();

        if (allEps.Count > 0)
        {
            return FindPrioritizedRegionalAlbum(allEps);
        }

        var allSingles = releaseList
            .Where(r => r.ReleaseGroup?.PrimaryType == "Single").ToList();

        if (allSingles.Count > 0)
        {
            return FindPrioritizedRegionalAlbum(allSingles);
        }

        return releaseList.First();
    }

    public static MbRelease FindPrioritizedRegionalAlbum(
        List<MbRelease> releases)
    {
        if (releases.Count == 1)
        {
            return releases.First();
        }

        return releases.FirstOrDefault(a => a.Country == "XW")
               ?? releases.FirstOrDefault(a => a.Country == "US")
               ?? releases.FirstOrDefault(a => a.Country == "GB")
               ?? releases.First();
    }
};