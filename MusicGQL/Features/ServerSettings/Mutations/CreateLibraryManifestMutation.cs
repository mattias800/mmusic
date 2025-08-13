using MusicGQL.Types;

namespace MusicGQL.Features.ServerSettings.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class CreateLibraryManifestMutation
{
    public async Task<CreateLibraryManifestResult> CreateLibraryManifest(
        CreateLibraryManifestInput input,
        [Service] LibraryManifestService manifestService
    )
    {
        if (string.IsNullOrWhiteSpace(input.LibraryPath))
            return new CreateLibraryManifestError("Library path is required");

        try
        {
            await manifestService.CreateManifestAsync(input.LibraryPath);
            return new CreateLibraryManifestSuccess(true);
        }
        catch (Exception ex)
        {
            return new CreateLibraryManifestError(ex.Message);
        }
    }
}

public record CreateLibraryManifestInput(string LibraryPath);

[UnionType("CreateLibraryManifestResult")]
public abstract record CreateLibraryManifestResult;
public record CreateLibraryManifestSuccess(bool Created) : CreateLibraryManifestResult;
public record CreateLibraryManifestError(string Message) : CreateLibraryManifestResult;


