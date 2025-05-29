using MusicGQL.Db.Postgres;
using MusicGQL.Features.ServerLibrary.Artist.Db;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Integration.Neo4j;

namespace MusicGQL.Features.ServerLibrary.Artist;

public record ArtistSearchRoot
{
    public async Task<IEnumerable<Artist>> All(
        ServerLibraryService service,
        EventDbContext dbContext
    )
    {
        var addedArtists = await dbContext.ArtistsAddedToServerLibraryProjections.FindAsync(1);

        if (addedArtists is null)
        {
            return [];
        }

        var allArtists = await service.GetArtistsByIdsAsync(addedArtists.ArtistMbIds);

        return allArtists.Select(a => new Artist(a));
    }

    public async Task<Artist?> ById(
        ServerLibraryService service,
        MusicBrainzService mbService,
        [ID] string id
    )
    {
        var artist = await service.GetArtistByIdAsync(id);
        if (artist is null)
        {
            var a = await mbService.GetArtistByIdAsync(id);
            if (a is null)
            {
                return null;
            }

            return new(
                new DbArtist
                {
                    Id = a.Id,
                    Name = a.Name,
                    SortName = a.SortName,
                    Gender = a.Gender,
                }
            );
        }

        return new(artist);
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
