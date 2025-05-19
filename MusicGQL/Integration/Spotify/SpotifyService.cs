using Microsoft.Extensions.Caching.Hybrid;
using SpotifyAPI.Web;

namespace MusicGQL.Integration.Spotify;

public class SpotifyService(SpotifyClient client, HybridCache cache) : CachedService(cache)
{
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
}
