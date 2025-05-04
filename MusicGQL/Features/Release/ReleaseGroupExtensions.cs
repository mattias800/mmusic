using MbReleaseGroup = Hqub.MusicBrainz.Entities.ReleaseGroup;

namespace MusicGQL.Features.Release;

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
}
