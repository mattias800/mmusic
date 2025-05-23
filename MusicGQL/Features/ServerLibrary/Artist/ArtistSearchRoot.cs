using MusicGQL.Integration.Neo4j;

namespace MusicGQL.Features.ServerLibrary.Artist;

public record ArtistSearchRoot
{
    public async Task<IEnumerable<Artist>> All(Neo4jService service)
    {
        var allArtists = await service.AllArtists(25, 0);
        return allArtists.Select(a => new Artist(a));
    }

    public async Task<IEnumerable<Artist>> SearchByName(Neo4jService service, string name)
    {
        var artists = await service.SearchArtistByNameAsync(name, 25, 0);
        return artists.Select(a => new Artist(a));
    }
}
