using MusicGQL.Features.External;
using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ListenBrainz;

namespace MusicGQL.Features.Artists;

public record Artist([property: GraphQLIgnore] CachedArtist Model) : IArtistBase
{
    [ID]
    public string Id() => Model.Id;

    public string Name() => Model.Name;

    public string SortName() => Model.SortName ?? Model.Name;

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
        Model.JsonArtist.TopTracks?
            .Select((t, index) => new ArtistTopTrack(Model.Id, t, index)) ?? [];

    public IEnumerable<ArtistConnectedExternalService> ConnectedExternalServices()
    {
        var connections = Model.JsonArtist.Connections;
        IEnumerable<ArtistConnectedExternalService> connectedExternalServices =
        [
            new(ExternalServiceCatalog.Musicbrainz(), connections?.MusicBrainzArtistId, connections),
            new(ExternalServiceCatalog.Spotify(), connections?.SpotifyId, connections),
            new(ExternalServiceCatalog.Apple(), connections?.AppleMusicArtistId, connections),
            new(ExternalServiceCatalog.Youtube(), connections?.YoutubeChannelUrl, connections),
            new(ExternalServiceCatalog.Tidal(), connections?.TidalArtistId, connections),
            new(ExternalServiceCatalog.Deezer(), connections?.DeezerArtistId, connections),
            new(ExternalServiceCatalog.Soundcloud(), connections?.SoundcloudUrl, connections),
            new(ExternalServiceCatalog.Bandcamp(), connections?.BandcampUrl, connections),
            new(ExternalServiceCatalog.Discogs(), connections?.DiscogsUrl, connections),
        ];

        return connectedExternalServices.Where(p => p.Model.Enabled);
    }

    public ArtistImages? Images()
    {
        if (Model.JsonArtist.Photos == null)
        {
            return null;
        }

        return new ArtistImages(Model.JsonArtist.Photos, Model.Id);
    }

    public IEnumerable<ArtistAppearsOn> AlsoAppearsOn()
    {
        if (Model.JsonArtist.AlsoAppearsOn == null || Model.JsonArtist.AlsoAppearsOn.Count == 0)
        {
            return [];
        }

        return Model.JsonArtist.AlsoAppearsOn.Select(appearance => new ArtistAppearsOn(appearance, Model.Id));
    }

    public IEnumerable<SimilarArtist> SimilarArtists()
    {
        var list = Model.JsonArtist.SimilarArtists;
        if (list == null || list.Count == 0)
        {
            return [];
        }
        return list.Select(sa => new SimilarArtist(sa));
    }
}