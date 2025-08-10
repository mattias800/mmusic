using MusicGQL.Features.Playlists.Import;

namespace MusicGQL.Features.Playlists;

public record PlaylistSearchRoot
{
    public ImportPlaylistSearchRoot ImportPlaylists => new();

    public async Task<Playlist?> ById(
        MusicGQL.Db.Postgres.EventDbContext db,
        [ID] string playlistId
    )
    {
        var pl = await db.Playlists.FindAsync(playlistId);
        return pl is null ? null : new Playlist(pl);
    }
}
