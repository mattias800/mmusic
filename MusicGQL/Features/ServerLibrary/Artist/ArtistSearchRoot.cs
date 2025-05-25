using MusicGQL.Db.Postgres;
using MusicGQL.Integration.Neo4j;

namespace MusicGQL.Features.ServerLibrary.Artist;

public record ArtistSearchRoot
{
    public async Task<IEnumerable<Artist>> All(
        ServerLibraryService service,
        EventDbContext dbContext
    )
    {
        var addedArtists = await dbContext.ArtistsAddedToServerLibraryProjection.FindAsync(1);

        if (addedArtists is null)
        {
            return [];
        }

        var allArtists = await service.GetArtistsByIdsAsync(addedArtists.ArtistMbIds);

        return allArtists.Select(a => new Artist(a));
    }

    public async Task<Artist?> ById(ServerLibraryService service, [ID] string id)
    {
        var artist = await service.GetArtistByIdAsync(id);
        return artist is null ? null : new(artist);
    }

    public async Task<IEnumerable<Artist>> SearchByName(
        ServerLibraryService service,
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var artists = await service.SearchArtistByNameAsync(name, limit, offset);
        return artists.Select(a => new Artist(a));
    }
}
