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

    public IEnumerable<ConnectedExternalService> ConnectedExternalServices()
    {
        var c = Model.JsonArtist.Connections;
        if (c is null) yield break;

        if (!string.IsNullOrWhiteSpace(c.MusicBrainzArtistId))
            yield return new ConnectedExternalService("musicbrainz", true);
        else
            yield return new ConnectedExternalService("musicbrainz", false);

        yield return new ConnectedExternalService("spotify", !string.IsNullOrWhiteSpace(c.SpotifyId));
        yield return new ConnectedExternalService("apple-music", !string.IsNullOrWhiteSpace(c.AppleMusicArtistId));
        yield return new ConnectedExternalService("youtube", !string.IsNullOrWhiteSpace(c.YoutubeChannelUrl));
        yield return new ConnectedExternalService("tidal", !string.IsNullOrWhiteSpace(c.TidalArtistId));
        yield return new ConnectedExternalService("deezer", !string.IsNullOrWhiteSpace(c.DeezerArtistId));
        yield return new ConnectedExternalService("soundcloud", !string.IsNullOrWhiteSpace(c.SoundcloudUrl));
        yield return new ConnectedExternalService("bandcamp", !string.IsNullOrWhiteSpace(c.BandcampUrl));
        yield return new ConnectedExternalService("discogs", !string.IsNullOrWhiteSpace(c.DiscogsUrl));
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