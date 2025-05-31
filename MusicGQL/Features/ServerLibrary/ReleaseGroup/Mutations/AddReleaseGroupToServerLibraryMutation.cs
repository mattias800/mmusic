using System.Security.Claims;
// Required for IHttpContextAccessor
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Handlers;
using MusicGQL.Types;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class AddReleaseGroupToServerLibraryMutation
{
    public async Task<AddReleaseGroupToServerLibraryResult> AddReleaseGroupToServerLibrary(
        [Service] MarkReleaseGroupAsAddedToServerLibraryHandler handler,
        AddReleaseGroupToServerLibraryInput input,
        [Service] IHttpContextAccessor httpContextAccessor // Inject IHttpContextAccessor
    )
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );
        if (!Guid.TryParse(userIdString, out var userId))
        {
            // Handle error: User not found or not authenticated
            return new AddReleaseGroupToServerLibraryResult.AddReleaseGroupToServerLibraryUnknownError(
                "User not authenticated or invalid user ID."
            );
        }

        return await handler.Handle(new(userId, input.ReleaseGroupId)) switch
        {
            // TODO Correct user
            MarkReleaseGroupAsAddedToServerLibraryHandler.Result.Success =>
                new AddReleaseGroupToServerLibraryResult.AddReleaseGroupToServerLibrarySuccess(
                    true
                ),
            MarkReleaseGroupAsAddedToServerLibraryHandler.Result.AlreadyAdded =>
                new AddReleaseGroupToServerLibraryResult.AddReleaseGroupToServerLibraryReleaseGroupAlreadyAdded(
                    "Release group already added!"
                ),
            MarkReleaseGroupAsAddedToServerLibraryHandler.Result.ReleaseGroupDoesNotExist =>
                new AddReleaseGroupToServerLibraryResult.AddReleaseGroupToServerLibraryReleaseGroupDoesNotExist(
                    "Release group does not exist in MusicBrainz!"
                ),
            _ =>
                new AddReleaseGroupToServerLibraryResult.AddReleaseGroupToServerLibraryUnknownError(
                    "Unhandled result."
                ),
        };
    }
};

public record AddReleaseGroupToServerLibraryInput([property: ID] string ReleaseGroupId);

[UnionType("AddReleaseGroupToServerLibraryResult")]
public abstract record AddReleaseGroupToServerLibraryResult
{
    public record AddReleaseGroupToServerLibrarySuccess(bool Success)
        : AddReleaseGroupToServerLibraryResult;

    public record AddReleaseGroupToServerLibraryReleaseGroupAlreadyAdded(string Message)
        : AddReleaseGroupToServerLibraryResult;

    public record AddReleaseGroupToServerLibraryReleaseGroupDoesNotExist(string Message)
        : AddReleaseGroupToServerLibraryResult;

    public record AddReleaseGroupToServerLibraryUnknownError(string Message)
        : AddReleaseGroupToServerLibraryResult;
}
