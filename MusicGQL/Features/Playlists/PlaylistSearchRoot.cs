using MusicGQL.Features.Playlists.Import;

namespace MusicGQL.Features.Playlists;

public record PlaylistSearchRoot
{
    public ImportPlaylistSearchRoot ImportPlaylists => new();

    [GraphQLName("playlist")]
    public async Task<Playlist?> PlaylistById(
        [Service] MusicGQL.Db.Postgres.EventDbContext db,
        [ID] string playlistId
    )
    {
        if (!Guid.TryParse(playlistId, out var id)) return null;
        var pl = await db.Playlists.FindAsync(id);
        return pl is null ? null : new Playlist(pl);
    }
}
