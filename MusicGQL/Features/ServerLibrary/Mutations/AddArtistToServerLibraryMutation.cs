using System.Security.Claims;
using MusicGQL.Features.Import.Handlers;
using MusicGQL.Types;

// Required for IHttpContextAccessor

namespace MusicGQL.Features.ServerLibrary.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class AddArtistToServerLibraryMutation
{
    public async Task<AddArtistToServerLibraryResult> AddArtistToServerLibrary(
        MarkArtistAsAddedToServerLibraryHandler markArtistAsAddedToServerLibraryHandler,
        MarkArtistReleaseGroupsAsAddedToServerLibraryHandler markArtistReleaseGroupsAsAddedToServerLibraryHandler,
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

        return await markArtistAsAddedToServerLibraryHandler.Handle(
            new(userId, input.ArtistId)
        ) switch
        {
            MarkArtistAsAddedToServerLibraryHandler.Result.Success => await HandleSuccess(
                markArtistReleaseGroupsAsAddedToServerLibraryHandler,
                importArtistToServerLibraryHandler,
                importArtistReleaseGroupsToServerLibraryHandler,
                userId,
                input.ArtistId,
                new AddArtistToServerLibraryResult.AddArtistToServerLibrarySuccess(true)
            ),
            MarkArtistAsAddedToServerLibraryHandler.Result.AlreadyAdded => await HandleSuccess(
                markArtistReleaseGroupsAsAddedToServerLibraryHandler,
                importArtistToServerLibraryHandler,
                importArtistReleaseGroupsToServerLibraryHandler,
                userId,
                input.ArtistId,
                new AddArtistToServerLibraryResult.AddArtistToServerLibraryArtistAlreadyAdded(
                    "Artist already added!"
                )
            ),
            MarkArtistAsAddedToServerLibraryHandler.Result.ArtistDoesNotExist =>
                new AddArtistToServerLibraryResult.AddArtistToServerLibraryArtistDoesNotExist(
                    "Artist does not exist in MusicBrainz!"
                ),
            _ => new AddArtistToServerLibraryResult.AddArtistToServerLibraryUnknownError(
                "Unhandled result."
            ),
        };
    }

    private async Task<AddArtistToServerLibraryResult> HandleSuccess(
        MarkArtistReleaseGroupsAsAddedToServerLibraryHandler markArtistReleaseGroupsAsAddedToServerLibraryHandler,
        ImportArtistToServerLibraryHandler importArtistToServerLibraryHandler,
        ImportArtistReleaseGroupsToServerLibraryHandler importArtistReleaseGroupsToServerLibraryHandler,
        Guid userId,
        string artistId,
        AddArtistToServerLibraryResult success
    )
    {
        await markArtistReleaseGroupsAsAddedToServerLibraryHandler.Handle(new(userId, artistId));
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
