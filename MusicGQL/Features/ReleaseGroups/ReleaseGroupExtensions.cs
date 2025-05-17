using MbReleaseGroup = Hqub.MusicBrainz.Entities.ReleaseGroup;

namespace MusicGQL.Features.ReleaseGroups;

public static class ReleaseGroupExtensions
{
    public static bool IsAlbum(this MbReleaseGroup releaseGroup)
    {
        return releaseGroup.PrimaryType == "Album";
    }

    public static bool IsMainAlbum(this MbReleaseGroup releaseGroup)
    {
        return releaseGroup.PrimaryType == "Album" && releaseGroup.SecondaryTypes.Count == 0;
    }

    public static bool IsMainSingle(this MbReleaseGroup releaseGroup)
    {
        return releaseGroup.PrimaryType == "Single" && releaseGroup.SecondaryTypes.Count == 0;
    }
}
