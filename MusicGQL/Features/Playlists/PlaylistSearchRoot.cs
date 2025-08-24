using Microsoft.EntityFrameworkCore;
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

    public async Task<List<Playlist>> SearchPlaylists(
        MusicGQL.Db.Postgres.EventDbContext db,
        int limit,
        string searchTerm
    )
    {
        var query = db
            .Playlists.Where(p => p.Name != null && p.Name.ToLower().Contains(searchTerm.ToLower()))
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit);
        var results = await query.ToListAsync();
        return results.Select(p => new Playlist(p)).ToList();
    }
}
