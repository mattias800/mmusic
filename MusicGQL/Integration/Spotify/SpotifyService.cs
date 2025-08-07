using Microsoft.Extensions.Caching.Hybrid;
using SpotifyAPI.Web;

namespace MusicGQL.Integration.Spotify;

public class SpotifyService(SpotifyClient client, HybridCache cache) : CachedService(cache)
{
    public async Task<FullPlaylist?> GetPlaylistDetailsAsync(string playlistId)
    {
        return await ExecuteThrottledAsync(
            $"spotify:playlist:{playlistId}:details",
            TimeSpan.FromDays(1),
            async () => await client.Playlists.Get(playlistId)
        );
    }

    public async Task<IEnumerable<FullTrack>?> GetTracksFromPlaylist(string playlistId)
    {
        return await ExecuteThrottledAsync(
            $"spotify:playlist:{playlistId}:tracks",
            TimeSpan.FromDays(1),
            async () =>
            {
                var fullPlaylist = await client.Playlists.Get(playlistId);
                return fullPlaylist.Tracks?.Items?.Select(item => item.Track).OfType<FullTrack>()
                    ?? [];
            }
        );
    }

    public async Task<IList<FullPlaylist>?> GetPlaylistsForUser(string userId)
    {
        return await ExecuteThrottledAsync(
            $"spotify:user:{userId}:playlists",
            TimeSpan.FromDays(1),
            async () =>
            {
                var pagingPlaylists = await client.Playlists.GetUsers(userId);
                return await client.PaginateAll(pagingPlaylists);
            }
        );
    }

    public async Task<List<FullArtist>> SearchArtistsAsync(string artistName, int limit = 20)
    {
        var searchRequest = new SearchRequest(SearchRequest.Types.Artist, artistName);
        var searchResponse = await client.Search.Item(searchRequest);

        return searchResponse.Artists.Items?.Take(limit).ToList() ?? [];
    }

    public async Task<FullArtist?> GetArtistAsync(string artistId)
    {
        return await ExecuteThrottledAsync(
            $"spotify:artist:{artistId}",
            TimeSpan.FromDays(7),
            async () => await client.Artists.Get(artistId)
        );
    }

    public async Task<FullAlbum?> GetAlbumAsync(string albumId)
    {
        return await ExecuteThrottledAsync(
            $"spotify:album:{albumId}",
            TimeSpan.FromDays(7),
            async () => await client.Albums.Get(albumId)
        );
    }

    public async Task<FullTrack?> GetTrackAsync(string trackId)
    {
        return await ExecuteThrottledAsync(
            $"spotify:track:{trackId}",
            TimeSpan.FromDays(7),
            async () => await client.Tracks.Get(trackId)
        );
    }

    public async Task<IList<SimpleAlbum>?> GetArtistAlbumsAsync(string artistId)
    {
        return await ExecuteThrottledAsync(
            $"spotify:artist:{artistId}:albums",
            TimeSpan.FromDays(7),
            async () =>
            {
                var pagingAlbums = await client.Artists.GetAlbums(artistId);
                return await client.PaginateAll(pagingAlbums);
            }
        );
    }

    public async Task<IList<SimpleTrack>?> GetAlbumTracksAsync(string albumId)
    {
        return await ExecuteThrottledAsync(
            $"spotify:album:{albumId}:tracks",
            TimeSpan.FromDays(7),
            async () =>
            {
                var pagingTracks = await client.Albums.GetTracks(albumId);
                return await client.PaginateAll(pagingTracks);
            }
        );
    }

    public async Task<List<FullTrack>?> GetArtistTopTracksAsync(
        string artistId,
        string market = "US"
    )
    {
        return await ExecuteThrottledAsync(
            $"spotify:artist:{artistId}:top_tracks:{market}",
            TimeSpan.FromDays(7),
            async () =>
            {
                var topTracks = await client.Artists.GetTopTracks(
                    artistId,
                    new ArtistsTopTracksRequest(market)
                );
                return topTracks.Tracks;
            }
        );
    }

    public async Task<List<FullArtist>?> GetArtistRelatedArtistsAsync(string artistId)
    {
        return await ExecuteThrottledAsync(
            $"spotify:artist:{artistId}:related",
            TimeSpan.FromDays(7),
            async () =>
            {
                var related = await client.Artists.GetRelatedArtists(artistId);
                return related.Artists;
            }
        );
    }
}
