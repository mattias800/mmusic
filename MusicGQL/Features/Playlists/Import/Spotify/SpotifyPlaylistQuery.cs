using MusicGQL.Integration.Spotify;

namespace MusicGQL.Features.Playlists.Import.Spotify;

public class SpotifyPlaylistSearchRoot
{
    public async Task<IEnumerable<SpotifyPlaylist>> SpotifyPlaylistsForUser(
        [Service] SpotifyService spotifyService,
        string username
    )
    {
        var playlists = await spotifyService.GetPlaylistsForUser(username) ?? [];
        return playlists.Select(p => new SpotifyPlaylist(p));
    }
}
