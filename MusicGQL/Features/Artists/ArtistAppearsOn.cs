using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Playlists;
using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.Artists;

public record ArtistAppearsOn([property: GraphQLIgnore] CachedArtist Artist)
{
    public IEnumerable<ArtistAppearsOnRelease> Releases()
    {
        var list = Artist.JsonArtist.AlsoAppearsOn;
        if (list == null || list.Count == 0)
        {
            return [];
        }

        return list.Select(appearance => new ArtistAppearsOnRelease(appearance, Artist.Id));
    }

    public async Task<IEnumerable<Playlist>> Playlists(
        [Service] IDbContextFactory<EventDbContext> dbFactory
    )
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var playlists = await db.Set<Features.Playlists.Db.DbPlaylist>()
            .Where(p => p.Items.Any(i => i.LocalArtistId == Artist.Id))
            .OrderByDescending(p => p.ModifiedAt ?? p.CreatedAt)
            .ToListAsync();

        return playlists.Select(p => new Playlist(p));
    }
}
