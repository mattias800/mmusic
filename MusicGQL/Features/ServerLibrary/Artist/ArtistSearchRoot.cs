using MusicGQL.Features.ServerLibrary.Artist.Db;
using Neo4j.Driver;

namespace MusicGQL.Features.ServerLibrary.Artist;

public record ArtistSearchRoot
{
    public async Task<IEnumerable<Artist>> All(IDriver driver)
    {
        await using var session = driver.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (a:Artist) RETURN a ORDER BY a.name ASC LIMIT 100"
        );

        var x = (await result.ToListAsync())?.Select(r => r.As<DbArtist>());
        return x?.Select(a => new Artist(a)) ?? [];
    }
}
