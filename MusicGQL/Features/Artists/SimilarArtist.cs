using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ServerLibrary.Utils;

namespace MusicGQL.Features.Artists;

public record SimilarArtist(
    [property: GraphQLIgnore] JsonSimilarArtist Model,
    [property: GraphQLIgnore] string ParentArtistId
)
{
    public string Name => Model.Name;
    public string? MusicBrainzArtistId => Model.MusicBrainzArtistId;
    public double? SimilarityScore => Model.SimilarityScore;
    public string? Thumb() =>
        !string.IsNullOrWhiteSpace(Model.MusicBrainzArtistId)
            ? LibraryAssetUrlFactory.CreateSimilarArtistThumbUrl(ParentArtistId, Model.MusicBrainzArtistId)
            : Model.Thumb;

    public async Task<Artist?> Artist(ServerLibraryCache cache)
    {
        if (string.IsNullOrWhiteSpace(Model.MusicBrainzArtistId))
            return null;

        var cached = await cache.GetArtistByMusicBrainzIdAsync(Model.MusicBrainzArtistId);
        return cached is null ? null : new Artist(cached);
    }
}


