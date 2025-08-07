using System.Security.Claims;
using MusicGQL.Features.Import.Handlers;
using MusicGQL.Types;

// Required for IHttpContextAccessor

namespace MusicGQL.Features.Import.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class AddArtistToServerLibraryMutation
{
    public async Task<AddArtistToServerLibraryResult> AddArtistToServerLibrary(
        LibraryImportService libraryImportService,
        ImportArtistToServerLibraryHandler importArtistToServerLibraryHandler,
        ImportArtistReleaseGroupsToServerLibraryHandler importArtistReleaseGroupsToServerLibraryHandler,
        AddArtistToServerLibraryInput input,
        [Service] IHttpContextAccessor httpContextAccessor // Inject IHttpContextAccessor
    )
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );
        if (!Guid.TryParse(userIdString, out var userId))
        {
            // Handle error: User not found or not authenticated
            return new AddArtistToServerLibraryResult.AddArtistToServerLibraryUnknownError(
                "User not authenticated or invalid user ID."
            );
        }

        return await HandleSuccess(
            importArtistToServerLibraryHandler,
            importArtistReleaseGroupsToServerLibraryHandler,
            userId,
            input.ArtistId,
            new AddArtistToServerLibraryResult.AddArtistToServerLibrarySuccess(true)
        );
    }

    private async Task<AddArtistToServerLibraryResult> HandleSuccess(
        ImportArtistToServerLibraryHandler importArtistToServerLibraryHandler,
        ImportArtistReleaseGroupsToServerLibraryHandler importArtistReleaseGroupsToServerLibraryHandler,
        Guid userId,
        string artistId,
        AddArtistToServerLibraryResult success
    )
    {
        await importArtistToServerLibraryHandler.Handle(new(artistId));
        _ = importArtistReleaseGroupsToServerLibraryHandler.Handle(new(artistId));
        return success;
    }
};

public record AddArtistToServerLibraryInput([property: ID] string ArtistId);

[UnionType("AddArtistToServerLibraryResult")]
public abstract record AddArtistToServerLibraryResult
{
    public record AddArtistToServerLibrarySuccess(bool Success) : AddArtistToServerLibraryResult;

    public record AddArtistToServerLibraryArtistAlreadyAdded(string Message)
        : AddArtistToServerLibraryResult;

    public record AddArtistToServerLibraryArtistDoesNotExist(string Message)
        : AddArtistToServerLibraryResult;

    public record AddArtistToServerLibraryUnknownError(string Message)
        : AddArtistToServerLibraryResult;
}
