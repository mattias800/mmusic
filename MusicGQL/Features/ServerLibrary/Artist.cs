using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;

namespace MusicGQL.Features.ServerLibrary;

public record Artist([property: GraphQLIgnore] CachedArtist Model)
{
    [ID]
    public string Id() => Model.Id;

    public string Name() => Model.Name;

    public string SortName() => Model.SortName ?? Model.Name;

    public ArtistServerStatus.ArtistServerStatus ServerStatus() => new(Model.Id);

    public async Task<IEnumerable<Release>> Releases(ServerLibraryCache cache)
    {
        var releases = await cache.GetAllReleasesForArtistAsync(Model.Id);
        return releases.Select(r => new Release(r));
    }

    public async Task<IEnumerable<Release>> Albums(ServerLibraryCache cache)
    {
        var releases = await cache.GetAllReleasesForArtistAsync(Model.Id);
        return releases.Where(r => r.Type == JsonReleaseType.Album).Select(r => new Release(r));
    }

    public async Task<IEnumerable<Release>> Singles(ServerLibraryCache cache)
    {
        var releases = await cache.GetAllReleasesForArtistAsync(Model.Id);
        return releases.Where(r => r.Type == JsonReleaseType.Single).Select(r => new Release(r));
    }

    public async Task<IEnumerable<Release>> Eps(ServerLibraryCache cache)
    {
        var releases = await cache.GetAllReleasesForArtistAsync(Model.Id);
        return releases.Where(r => r.Type == JsonReleaseType.Ep).Select(r => new Release(r));
    }

    public async Task<Release?> ReleaseByFolderName(
        ServerLibraryCache cache,
        string releaseFolderName
    )
    {
        var release = await cache.GetReleaseByArtistAndFolderAsync(Model.Id, releaseFolderName);
        return release == null ? null : new Release(release);
    }

    public long? Listeners() => Model.JsonArtist.MonthlyListeners;

    public IEnumerable<ArtistTopTrack> TopTracks() =>
        Model.JsonArtist.TopTracks?.Select(t => new ArtistTopTrack(Model.Id, t)) ?? [];

    public ArtistImages? Images()
    {
        if (Model.JsonArtist.Photos == null)
        {
            return null;
        }

        return new ArtistImages(Model.JsonArtist.Photos, Model.Id);
    }
}
