using MusicGQL.Integration.Spotify;

namespace MusicGQL.Features.Playlists.Import.Spotify;

public class SpotifyPlaylistSearchRoot
{
    public async Task<IEnumerable<SpotifyPlaylist>> SpotifyPlaylistsForUser(
        [Service] SpotifyService spotifyService,
        [Service] ILogger<SpotifyPlaylistSearchRoot> logger,
        string username
    )
    {
        try
        {
            var playlists = await spotifyService.GetPlaylistsForUser(username) ?? [];
            return playlists.Select(p => new SpotifyPlaylist(p));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch Spotify playlists for user {Username}", username);
            // Gracefully degrade to empty list to avoid Unexpected Execution Error in GraphQL
            return [];
        }
    }
}
