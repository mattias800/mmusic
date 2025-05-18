namespace MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

public class CoverArtArchive
{
    public bool Artwork { get; set; }
    public int? Count { get; set; }
    public bool Front { get; set; }
    public bool Back { get; set; }

    public static Uri GetCoverArtUri(string releaseId)
    {
        return new Uri(
            $"https://coverartarchive.org/release/{releaseId}/front-250.jpg",
            UriKind.RelativeOrAbsolute
        );
    }
}
