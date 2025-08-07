using MusicGQL.Features.ServerLibrary.Json;

namespace MusicGQL.Features.ServerLibrary;

public record ArtistImages(
    [property: GraphQLIgnore] ArtistPhotosJson Model,
    [property: GraphQLIgnore] string ArtistId
)
{
    /// <summary>
    /// Gets a list of thumbnail photo URLs
    /// </summary>
    public List<string> Thumbs() =>
        Model
            .Thumbs?.Select(
                (_, index) => $"/library/{Uri.EscapeDataString(ArtistId)}/photos/thumbs/{index}"
            )
            .ToList() ?? new List<string>();

    /// <summary>
    /// Gets a list of background photo URLs
    /// </summary>
    public List<string> Backgrounds() =>
        Model
            .Backgrounds?.Select(
                (_, index) =>
                    $"/library/{Uri.EscapeDataString(ArtistId)}/photos/backgrounds/{index}"
            )
            .ToList() ?? new List<string>();

    /// <summary>
    /// Gets a list of banner photo URLs
    /// </summary>
    public List<string> Banners() =>
        Model
            .Banners?.Select(
                (_, index) => $"/library/{Uri.EscapeDataString(ArtistId)}/photos/banners/{index}"
            )
            .ToList() ?? new List<string>();

    /// <summary>
    /// Gets a list of logo photo URLs
    /// </summary>
    public List<string> Logos() =>
        Model
            .Logos?.Select(
                (_, index) => $"/library/{Uri.EscapeDataString(ArtistId)}/photos/logos/{index}"
            )
            .ToList() ?? new List<string>();
}
