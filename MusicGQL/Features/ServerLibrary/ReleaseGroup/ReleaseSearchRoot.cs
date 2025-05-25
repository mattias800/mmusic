using MusicGQL.Integration.Neo4j;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup;

public record ReleaseGroupSearchRoot
{
    public async Task<IEnumerable<ReleaseGroup>> All(ServerLibraryService service)
    {
        var releaseGroups = await service.GetAllReleaseGroupAsync();
        return releaseGroups.Select(a => new ReleaseGroup(a));
    }

    public async Task<ReleaseGroup?> ById(ServerLibraryService service, [ID] string id)
    {
        var releaseGroup = await service.GetReleaseGroupByIdAsync(id);
        return releaseGroup is null ? null : new(releaseGroup);
    }

    public async Task<IEnumerable<ReleaseGroup>> SearchByName(
        ServerLibraryService service,
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var releases = await service.SearchReleaseGroupByNameAsync(name, limit, offset);
        return releases.Select(a => new ReleaseGroup(a));
    }
}
