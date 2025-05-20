using MusicGQL.Features.Playlists.Import;

namespace MusicGQL.Features.Playlists;

public record PlaylistSearchRoot
{
    public ImportPlaylistSearchRoot ImportPlaylists => new();
}
