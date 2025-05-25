using MusicGQL.Integration.Neo4j;

namespace MusicGQL.Features.ServerLibrary.Release;

public record ReleaseSearchRoot
{
    public async Task<IEnumerable<Release>> SearchByName(
        ServerLibraryService service,
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var releases = await service.SearchReleaseByNameAsync(name, limit, offset);
        return releases.Select(a => new Release(a));
    }

    public async Task<Release?> ById(ServerLibraryService service, [ID] string id)
    {
        try
        {
            var recording = await service.GetReleaseByIdAsync(id);
            return recording != null ? new Release(recording) : null;
        }
        catch
        {
            return null;
        }
    }
};
