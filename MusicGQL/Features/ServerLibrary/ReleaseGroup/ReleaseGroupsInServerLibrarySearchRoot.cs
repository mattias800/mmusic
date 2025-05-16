using MusicGQL.Db;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup;

public record ReleaseGroupsInServerLibrarySearchRoot()
{
    public async Task<IEnumerable<ReleaseGroupInServerLibrary>> All(EventDbContext dbContext)
    {
        var p = await dbContext.ReleaseGroupsAddedToServerLibraryProjection.FindAsync(1);
        return p?.ReleaseGroupMbIds.Select(mbId => new ReleaseGroupInServerLibrary(mbId)) ?? [];
    }
};
