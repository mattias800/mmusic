using MusicGQL.Integration.Neo4j;

namespace MusicGQL.Features.ServerLibrary.Artist;

public record ArtistSearchRoot
{
    public async Task<IEnumerable<Artist>> All(Neo4jService service)
    {
        var allArtists = await service.AllArtists(25, 0);
        return allArtists.Select(a => new Artist(a));
    }

    public async Task<Artist?> ById(Neo4jService service, [ID] string id)
    {
        var artist = await service.GetArtistByIdAsync(id);
        return artist is null ? null : new(artist);
    }

    public async Task<IEnumerable<Artist>> SearchByName(
        Neo4jService service,
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var artists = await service.SearchArtistByNameAsync(name, limit, offset);
        return artists.Select(a => new Artist(a));
    }
}
