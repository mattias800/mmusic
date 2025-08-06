using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.ServerLibrary;

public class ServerLibrarySearchRoot
{
    /// <summary>
    /// Get all artists from the cached library
    /// </summary>
    public async Task<IEnumerable<Artist>> AllArtists([Service] ServerLibraryCache cache)
    {
        var cachedArtists = await cache.GetAllArtistsAsync();
        return cachedArtists.Select(a => new Artist(a));
    }

    /// <summary>
    /// Get an artist by ID
    /// </summary>
    public async Task<Artist?> ArtistById([Service] ServerLibraryCache cache, [ID] string id)
    {
        var cachedArtist = await cache.GetArtistByIdAsync(id);
        return cachedArtist != null ? new Artist(cachedArtist) : null;
    }

    /// <summary>
    /// Search artists by name
    /// </summary>
    public async Task<IEnumerable<Artist>> SearchArtists(
        [Service] ServerLibraryCache cache,
        string searchTerm,
        int limit = 20
    )
    {
        var cachedArtists = await cache.SearchArtistsByNameAsync(searchTerm, limit);
        return cachedArtists.Select(a => new Artist(a));
    }

    /// <summary>
    /// Get all releases from the cached library
    /// </summary>
    public async Task<IEnumerable<Release>> AllReleases([Service] ServerLibraryCache cache)
    {
        var cachedReleases = await cache.GetAllReleasesAsync();
        return cachedReleases.Select(r => new Release(r));
    }

    /// <summary>
    /// Search releases by title
    /// </summary>
    public async Task<IEnumerable<Release>> SearchReleases(
        [Service] ServerLibraryCache cache,
        string searchTerm,
        int limit = 20
    )
    {
        var cachedReleases = await cache.SearchReleasesByTitleAsync(searchTerm, limit);
        return cachedReleases.Select(r => new Release(r));
    }

    /// <summary>
    /// Get all tracks from the cached library
    /// </summary>
    public async Task<IEnumerable<Track>> AllTracks([Service] ServerLibraryCache cache)
    {
        var cachedTracks = await cache.GetAllTracksAsync();
        return cachedTracks.Select(t => new Track(t));
    }

    /// <summary>
    /// Search tracks by title
    /// </summary>
    public async Task<IEnumerable<Track>> SearchTracks(
        [Service] ServerLibraryCache cache,
        string searchTerm,
        int limit = 20
    )
    {
        var cachedTracks = await cache.SearchTracksByTitleAsync(searchTerm, limit);
        return cachedTracks.Select(t => new Track(t));
    }

    /// <summary>
    /// Get releases for a specific artist by artist ID
    /// </summary>
    public async Task<IEnumerable<Release>> ReleasesForArtist(
        [Service] ServerLibraryCache cache,
        [ID] string artistId
    )
    {
        var artist = await cache.GetArtistByIdAsync(artistId);
        if (artist == null)
            return [];

        return artist.Releases.Select(r => new Release(r));
    }

    /// <summary>
    /// Get tracks for a specific artist by artist ID
    /// </summary>
    public async Task<IEnumerable<Track>> TracksForArtist(
        [Service] ServerLibraryCache cache,
        [ID] string artistId
    )
    {
        var artist = await cache.GetArtistByIdAsync(artistId);
        if (artist == null)
            return [];

        return artist.Releases.SelectMany(r => r.Tracks).Select(t => new Track(t));
    }

    /// <summary>
    /// Get cache statistics
    /// </summary>
    public async Task<CacheStatistics> LibraryStatistics([Service] ServerLibraryCache cache)
    {
        return await cache.GetCacheStatisticsAsync();
    }
}
