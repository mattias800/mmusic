using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ServerLibrary.Cache;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class ScanLibraryForMissingJsonMutation
{
    public async Task<ScanLibraryForMissingJsonResult> ScanLibraryForMissingJson(
        [Service] LibraryMaintenanceCoordinator coordinator
    )
    {
        var scan = await coordinator.RunAsync();
        return new ScanLibraryForMissingJsonSuccess(
            scan.Success,
            scan.ArtistsCreated,
            scan.ReleasesCreated,
            scan.Notes,
            scan.ErrorMessage
        );
    }
}

[UnionType("ScanLibraryForMissingJsonResult")]
public abstract record ScanLibraryForMissingJsonResult;

public record ScanLibraryForMissingJsonSuccess(
    bool Success,
    int ArtistsCreated,
    int ReleasesCreated,
    List<string> Notes,
    string? ErrorMessage
) : ScanLibraryForMissingJsonResult;

