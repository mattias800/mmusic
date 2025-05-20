using MusicGQL.Features.Playlists.Import.Spotify;

namespace MusicGQL.Features.Playlists.Import;

public record ImportPlaylistSearchRoot
{
    public SpotifyPlaylistSearchRoot Spotify => new();
}
