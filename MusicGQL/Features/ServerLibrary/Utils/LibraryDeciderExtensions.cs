namespace MusicGQL.Features.ServerLibrary.Utils;

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

    public static bool IsMainSingle(this Hqub.MusicBrainz.Entities.ReleaseGroup releaseGroup)
    {
        return releaseGroup.PrimaryType == "Single" && releaseGroup.SecondaryTypes.Count == 0;
    }

    public static bool IsMainEP(this Hqub.MusicBrainz.Entities.ReleaseGroup releaseGroup)
    {
        return releaseGroup.PrimaryType == "EP" && releaseGroup.SecondaryTypes.Count == 0;
    }

    public static bool IsDemo(this Hqub.MusicBrainz.Entities.ReleaseGroup releaseGroup)
    {
        // MusicBrainz marks demos as a secondary type "Demo"
        return releaseGroup.SecondaryTypes.Any(t =>
            t.Equals("Demo", StringComparison.OrdinalIgnoreCase)
        );
    }
}
