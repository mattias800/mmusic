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
            .Select(i => i.RecordingId)
            .ToListAsync();

        // RecordingId is currently a platform-specific external ID (e.g., Spotify);
        // We only include local library tracks that match our stable Track ID format: artistId/folder/trackNumber
        var tracks = new List<Track>();
        foreach (var recId in items)
        {
            var parts = recId.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 3 && int.TryParse(parts[2], out var trackNumber))
            {
                var cached = await cache.GetTrackByArtistReleaseAndNumberAsync(parts[0], parts[1], trackNumber);
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
