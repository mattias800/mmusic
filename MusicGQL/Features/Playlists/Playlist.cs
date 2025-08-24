using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;

namespace MusicGQL.Features.Playlists;

public record Playlist([property: GraphQLIgnore] Db.DbPlaylist Model)
{
    [ID]
    public string Id() => Model.Id;

    public string? Name() => Model.Name;

    public async Task<IEnumerable<PlaylistItem>> Items(
        [Service] IDbContextFactory<EventDbContext> dbFactory
    )
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var items = await db.Set<Db.DbPlaylistItem>()
            .Where(i => i.PlaylistId == Model.Id)
            .OrderBy(i => i.AddedAt)
            .ToListAsync();

        return items.Select(i => new PlaylistItem(i));
    }

    public DateTime CreatedAt() => Model.CreatedAt;

    public DateTime? ModifiedAt() => Model.ModifiedAt;

    public string? CoverImageUrl() => Model.CoverImageUrl;
}
