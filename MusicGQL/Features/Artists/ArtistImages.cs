using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ServerLibrary.Utils;

namespace MusicGQL.Features.Artists;

public record ArtistImages(
    [property: GraphQLIgnore] JsonArtistPhotos Model,
    [property: GraphQLIgnore] string ArtistId
)
{
    /// <summary>
    /// Gets a list of thumbnail photo URLs
    /// </summary>
    public List<string> Thumbs() =>
        LibraryAssetUrlFactory.CreateArtistPhotoUrls(ArtistId, "thumbs", Model.Thumbs?.Count ?? 0);

    /// <summary>
    /// Gets a list of background photo URLs
    /// </summary>
    public List<string> Backgrounds() =>
        LibraryAssetUrlFactory.CreateArtistPhotoUrls(
            ArtistId,
            "backgrounds",
            Model.Backgrounds?.Count ?? 0
        );

    /// <summary>
    /// Gets a list of banner photo URLs
    /// </summary>
    public List<string> Banners() =>
        LibraryAssetUrlFactory.CreateArtistPhotoUrls(
            ArtistId,
            "banners",
            Model.Banners?.Count ?? 0
        );

    /// <summary>
    /// Gets a list of logo photo URLs
    /// </summary>
    public List<string> Logos() =>
        LibraryAssetUrlFactory.CreateArtistPhotoUrls(ArtistId, "logos", Model.Logos?.Count ?? 0);
}
