using MusicGQL.Features.Playlists.Events;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Integration.Spotify;

namespace MusicGQL.Features.Artists;

public record ArtistSearchRoot
{
    public async Task<Artist?> ById(ServerLibraryCache cache, [ID] string artistId)
    {
        var artist = await cache.GetArtistByIdAsync(artistId);
        return artist == null ? null : new(artist);
    }

    public async Task<IEnumerable<Artist>> SearchArtists(
        [Service] ServerLibraryCache cache,
        string searchTerm,
        int limit = 20
    )
    {
        var cachedArtists = await cache.SearchArtistsByNameAsync(searchTerm, limit);
        return cachedArtists.Select(a => new Artist(a));
    }

    public async Task<IEnumerable<Artist>> All([Service] ServerLibraryCache cache)
    {
        var cachedArtists = await cache.GetAllArtistsAsync();
        return cachedArtists.Select(a => new Artist(a));
    }

    public async Task<ExternalArtist?> ExternalArtistById(
        [Service] SpotifyService spotify,
        [ID] string artistId,
        ExternalServiceType serviceType
    ) => serviceType == ExternalServiceType.Spotify
        ? await GetSpotifyExternalArtist(spotify, artistId)
        : null;

    public async Task<IEnumerable<ExternalArtist>> SearchExternalArtists(
        [Service] SpotifyService spotify,
        string searchTerm,
        ExternalServiceType serviceType,
        int limit = 10
    )
    {
        if (serviceType != ExternalServiceType.Spotify)
        {
            return Enumerable.Empty<ExternalArtist>();
        }

        var results = await spotify.SearchArtistsAsync(searchTerm, limit);
        return results.Select(a => new ExternalArtist(a.Id ?? string.Empty, a.Name ?? string.Empty, ExternalServiceType.Spotify));
    }

    private static async Task<ExternalArtist?> GetSpotifyExternalArtist(SpotifyService spotify, string artistId)
    {
        var a = await spotify.GetArtistAsync(artistId);
        return a is null ? null : new ExternalArtist(a.Id ?? string.Empty, a.Name ?? string.Empty, ExternalServiceType.Spotify);
    }
}