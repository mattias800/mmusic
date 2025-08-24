using MusicGQL.Features.ServerLibrary;
using MusicGQL.Types;

namespace MusicGQL.Features.ServerSettings.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class CreateLibraryManifestMutation
{
    public async Task<CreateLibraryManifestResult> CreateLibraryManifest(
        ServerSettingsAccessor serverSettingsAccessor,
        LibraryManifestService manifestService
    )
    {
        var settings = await serverSettingsAccessor.GetAsync();

        if (string.IsNullOrWhiteSpace(settings.LibraryPath))
        {
            return new CreateLibraryManifestError("Library path is required");
        }

        try
        {
            await manifestService.CreateManifestAsync(settings.LibraryPath);
            return new CreateLibraryManifestSuccess(new ServerLibraryManifestStatus());
        }
        catch (Exception ex)
        {
            return new CreateLibraryManifestError(ex.Message);
        }
    }
}

[UnionType("CreateLibraryManifestResult")]
public abstract record CreateLibraryManifestResult;

public record CreateLibraryManifestSuccess(ServerLibraryManifestStatus ServerLibraryManifestStatus)
    : CreateLibraryManifestResult;

public record CreateLibraryManifestError(string Message) : CreateLibraryManifestResult;
