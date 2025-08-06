using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;

namespace MusicGQL.Features.ServerLibrary;

public enum ReleaseType
{
    Album,
    Ep,
    Single,
}

public record Release(
    [property: GraphQLIgnore] ArtistJson ArtistJson,
    [property: GraphQLIgnore] ReleaseJson Model
)
{
    [ID]
    public string Id() => ArtistJson.Id + Model.Title;

    public string Title() => Model.Title;

    public ReleaseType? Type() =>
        Model.Type switch
        {
            Json.ReleaseType.Album => ReleaseType.Album,
            Json.ReleaseType.Ep => ReleaseType.Ep,
            Json.ReleaseType.Single => ReleaseType.Single,
            _ => null,
        };

    public string? FirstReleaseDate() => Model.FirstReleaseDate;

    public string? FirstReleaseYear() => Model.FirstReleaseDate?.Split("-").FirstOrDefault();

    // TODO: Return correct URL that the server can serve.
    public string? CoverArtUri() => Model.CoverArt;

    public async Task<IEnumerable<Track>> Recordings(ServerLibraryCache cache)
    {
        var recordings = await cache.GetAllTracksAsync();
        var sortedByTrackPosition = recordings
            .Select(r => r.TrackJson)
            .OrderBy(r => r.TrackNumber)
            .ToList();

        return sortedByTrackPosition.Select(r => new Track(r));
    }
};
