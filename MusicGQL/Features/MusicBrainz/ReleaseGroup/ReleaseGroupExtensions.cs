namespace MusicGQL.Features.MusicBrainz.ReleaseGroup;

public static class ReleaseGroupExtensions
{
    public static bool IsAlbum(this Hqub.MusicBrainz.Entities.ReleaseGroup releaseGroup)
    {
        return releaseGroup.PrimaryType == "Album";
    }

    public static bool IsMainAlbum(this Hqub.MusicBrainz.Entities.ReleaseGroup releaseGroup)
    {
        return releaseGroup.PrimaryType == "Album" && releaseGroup.SecondaryTypes.Count == 0;
    }

    public static bool IsMainSingle(this Hqub.MusicBrainz.Entities.ReleaseGroup releaseGroup)
    {
        return releaseGroup.PrimaryType == "Single" && releaseGroup.SecondaryTypes.Count == 0;
    }
}
