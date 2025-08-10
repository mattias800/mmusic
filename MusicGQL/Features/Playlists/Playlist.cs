using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Cache;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;

namespace MusicGQL.Features.Playlists;

public record Playlist([property: GraphQLIgnore] Db.DbPlaylist Model)
{
    [ID]
    public string Id() => Model.Id.ToString();

    public string? Name() => Model.Name;

    public async Task<IEnumerable<PlaylistItem>> Items(
        [Service] IDbContextFactory<EventDbContext> dbFactory
    )
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var items = await db
            .Set<Db.DbPlaylistItem>()
            .Where(i => i.PlaylistId == Model.Id)
            .OrderBy(i => i.AddedAt)
            .ToListAsync();

        return items.Select(i => new PlaylistItem(i));
    }

    // Backward-compatible field that returns only local library tracks from the playlist
    public async Task<IEnumerable<Track>> Tracks(
        [Service] IDbContextFactory<EventDbContext> dbFactory,
        [Service] ServerLibraryCache cache
    )
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var items = await db
            .Set<Db.DbPlaylistItem>()
            .Where(i => i.PlaylistId == Model.Id)
            .OrderBy(i => i.AddedAt)
            .ToListAsync();

        var tracks = new List<Track>();
        foreach (var item in items)
        {
            // Prefer explicit local reference
            if (
                !string.IsNullOrWhiteSpace(item.LocalArtistId)
                && !string.IsNullOrWhiteSpace(item.LocalReleaseFolderName)
                && item.LocalTrackNumber.HasValue
            )
            {
                var cached = await cache.GetTrackByArtistReleaseAndNumberAsync(
                    item.LocalArtistId!,
                    item.LocalReleaseFolderName!,
                    item.LocalTrackNumber!.Value
                );
                if (cached != null)
                {
                    tracks.Add(new Track(cached));
                    continue;
                }
            }

            // Fallback: parse recording id
            var parts = item.RecordingId.Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );
            if (parts.Length == 3 && int.TryParse(parts[2], out var trackNumber))
            {
                var cached = await cache.GetTrackByArtistReleaseAndNumberAsync(
                    parts[0],
                    parts[1],
                    trackNumber
                );
                if (cached != null)
                {
                    tracks.Add(new Track(cached));
                }
            }
        }

        return tracks;
    }

    public DateTime CreatedAt() => Model.CreatedAt;

    public DateTime? ModifiedAt() => Model.ModifiedAt;

    public string? CoverImageUrl() => Model.CoverImageUrl;
}
