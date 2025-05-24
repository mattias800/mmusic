using MusicGQL.Features.ServerLibrary.ReleaseGroup.Db;
using MbRelease = Hqub.MusicBrainz.Entities.Release;

namespace MusicGQL.Features.ServerLibrary;

public static class LibraryDecider
{
    public static bool ShouldBeAddedWhenAddingArtistToServerLibrary(
        Hqub.MusicBrainz.Entities.ReleaseGroup releaseGroup
    ) => releaseGroup.IsMainAlbum() || releaseGroup.IsMainEP() || releaseGroup.IsMainSingle();

    public static MbRelease? GetMainReleaseInReleaseGroup(List<MbRelease> releases)
    {
        var official = releases.Where(r => r.Status == "Official").ToList();

        var releasesAvailable = official.Count != 0 ? official : releases;

        var worldWideReleases = releasesAvailable.Where(r => r.Country == "XW").ToList();

        if (worldWideReleases.Any())
        {
            return worldWideReleases.Last();
        }

        return releasesAvailable // filter by official only
                .OrderBy(r =>
                    r.Country switch
                    {
                        "US" => 1,
                        "GB" => 2,
                        _ => 3,
                    }
                )
                .ThenBy(r => r.Date) // prefer earlier date
                .FirstOrDefault() ?? releases.FirstOrDefault();
    }

    public static MbRelease? FindMainAlbumForSong(List<MbRelease> releases)
    {
        var official = releases.Where(r => r.Status == "Official").ToList();

        var releasesAvailable = official.Count != 0 ? official : releases;

        var allAlbums = releasesAvailable.Where(r => r.ReleaseGroup.IsMainAlbum()).ToList();

        if (allAlbums.Count > 0)
        {
            return FindPrioritizedRegionalAlbum(allAlbums);
        }

        var allEps = releasesAvailable.Where(r => r.ReleaseGroup.IsMainEP()).ToList();

        if (allEps.Count > 0)
        {
            return FindPrioritizedRegionalAlbum(allEps);
        }

        var allSingles = releasesAvailable.Where(r => r.ReleaseGroup.IsMainSingle()).ToList();

        if (allSingles.Count > 0)
        {
            return FindPrioritizedRegionalAlbum(allSingles);
        }

        return releasesAvailable.First();
    }

    public static DbReleaseGroup? FindMainReleaseGroupForRecording(
        List<DbReleaseGroup> releaseGroups
    )
    {
        var album = releaseGroups.FirstOrDefault(r => r.IsMainAlbum());
        var ep = releaseGroups.FirstOrDefault(r => r.IsMainEP());
        var single = releaseGroups.FirstOrDefault(r => r.IsMainSingle());

        return album ?? ep ?? single ?? releaseGroups.FirstOrDefault();
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
}
