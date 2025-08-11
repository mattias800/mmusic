using MusicGQL.Features.External;
using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;

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
        IEnumerable<ArtistConnectedExternalService> connectedExternalServices =
        [
            new(ExternalServiceCatalog.Musicbrainz(), Model.JsonArtist.Connections?.MusicBrainzArtistId != null),
            new(ExternalServiceCatalog.Spotify(), Model.JsonArtist.Connections?.SpotifyId != null),
            new(ExternalServiceCatalog.Apple(), Model.JsonArtist.Connections?.AppleMusicArtistId != null),
            new(ExternalServiceCatalog.Youtube(), Model.JsonArtist.Connections?.YoutubeChannelUrl != null),
            new(ExternalServiceCatalog.Tidal(), Model.JsonArtist.Connections?.TidalArtistId != null),
            new(ExternalServiceCatalog.Deezer(), Model.JsonArtist.Connections?.DeezerArtistId != null),
            new(ExternalServiceCatalog.Soundcloud(), Model.JsonArtist.Connections?.SoundcloudUrl != null),
            new(ExternalServiceCatalog.Bandcamp(), Model.JsonArtist.Connections?.BandcampUrl != null),
            new(ExternalServiceCatalog.Discogs(), Model.JsonArtist.Connections?.DiscogsUrl != null),
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
}