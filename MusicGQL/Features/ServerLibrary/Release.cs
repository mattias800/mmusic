using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.ServerLibrary;

public enum ReleaseType
{
    Album,
    Ep,
    Single,
}

public record Release([property: GraphQLIgnore] CachedRelease Model)
{
    [ID]
    public string Id() => Model.ArtistId + "/" + Model.SearchFolderName;

    public string Title() => Model.Title;

    public ReleaseType? Type() =>
        Model.Type switch
        {
            Json.ReleaseType.Album => ReleaseType.Album,
            Json.ReleaseType.Ep => ReleaseType.Ep,
            Json.ReleaseType.Single => ReleaseType.Single,
            _ => null,
        };

    public string? FirstReleaseDate() => Model.ReleaseJson.FirstReleaseDate;

    public string? FirstReleaseYear() =>
        Model.ReleaseJson.FirstReleaseDate?.Split("-").FirstOrDefault();

    /// <summary>
    /// Gets the cover art URL that the server can serve
    /// </summary>
    public string CoverArtUrl() => 
        LibraryAssetUrlFactory.CreateReleaseCoverArtUrl(Model.ArtistId, Model.FolderName);

    public async Task<IEnumerable<Track>> Tracks(ServerLibraryCache cache)
    {
        var tracks = await cache.GetAllTracksForReleaseAsync(Model.ArtistId, Model.FolderName);
        var sortedByTrackPosition = tracks.OrderBy(r => r.TrackJson.TrackNumber).ToList();

        return sortedByTrackPosition.Select(r => new Track(r));
    }
};
