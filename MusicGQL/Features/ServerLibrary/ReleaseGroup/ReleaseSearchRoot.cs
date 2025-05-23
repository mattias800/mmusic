using MusicGQL.Features.ServerLibrary.ReleaseGroup.Db;
using Neo4j.Driver;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup;

public record ReleaseGroupSearchRoot
{
    public async Task<IEnumerable<ReleaseGroup>> All(IDriver driver)
    {
        await using var session = driver.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (a:ReleaseGroup) RETURN a ORDER BY a.name ASC LIMIT 100"
        );

        var x = (await result.ToListAsync())?.Select(r => r.As<DbReleaseGroup>());
        return x?.Select(a => new ReleaseGroup(a)) ?? [];
    }

    public async Task<ReleaseGroup?> ById(IDriver driver)
    {
        await using var session = driver.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (a:ReleaseGroup) RETURN a ORDER BY a.name ASC LIMIT 100"
        );

        return result.Current is null ? null : result.As<ReleaseGroup>();
    }
}
