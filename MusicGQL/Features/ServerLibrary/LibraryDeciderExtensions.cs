using MusicGQL.Features.ServerLibrary.ReleaseGroup.Db;

namespace MusicGQL.Features.ServerLibrary;

public static class LibraryDeciderExtensions
{
    public static bool IsAlbum(this Hqub.MusicBrainz.Entities.ReleaseGroup releaseGroup)
    {
        return releaseGroup.PrimaryType == "Album";
    }

    public static bool IsMainAlbum(this Hqub.MusicBrainz.Entities.ReleaseGroup releaseGroup)
    {
        return releaseGroup.PrimaryType == "Album" && releaseGroup.SecondaryTypes.Count == 0;
    }

    public static bool IsMainAlbum(this DbReleaseGroup releaseGroup)
    {
        return releaseGroup is { PrimaryType: "Album", SecondaryTypes.Count: 0 };
    }

    public static bool IsMainSingle(this Hqub.MusicBrainz.Entities.ReleaseGroup releaseGroup)
    {
        return releaseGroup.PrimaryType == "Single" && releaseGroup.SecondaryTypes.Count == 0;
    }

    public static bool IsMainSingle(this DbReleaseGroup releaseGroup)
    {
        return releaseGroup is { PrimaryType: "Single", SecondaryTypes.Count: 0 };
    }

    public static bool IsMainEP(this Hqub.MusicBrainz.Entities.ReleaseGroup releaseGroup)
    {
        return releaseGroup.PrimaryType == "EP" && releaseGroup.SecondaryTypes.Count == 0;
    }

    public static bool IsMainEP(this DbReleaseGroup releaseGroup)
    {
        return releaseGroup is { PrimaryType: "EP", SecondaryTypes.Count: 0 };
    }
}
