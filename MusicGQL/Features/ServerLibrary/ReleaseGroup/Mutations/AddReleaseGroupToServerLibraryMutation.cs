using MusicGQL.Features.ServerLibrary.ReleaseGroup.Handlers;
using MusicGQL.Types;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class AddReleaseGroupToServerLibraryMutation
{
    public async Task<AddReleaseGroupToServerLibraryResult> AddReleaseGroupToServerLibrary(
        [Service] AddReleaseGroupToServerLibraryHandler handler,
        AddReleaseGroupToServerLibraryInput input
    )
    {
        return await handler.Handle(new(input.ReleaseGroupId)) switch
        {
            // TODO Correct user
            AddReleaseGroupToServerLibraryHandler.Result.Success =>
                new AddReleaseGroupToServerLibraryResult.AddReleaseGroupToServerLibrarySuccess(
                    new User.User(0)
                ),
            AddReleaseGroupToServerLibraryHandler.Result.AlreadyAdded =>
                new AddReleaseGroupToServerLibraryResult.AddReleaseGroupToServerLibraryReleaseGroupAlreadyAdded(
                    "ReleaseGroup already added!"
                ),
            AddReleaseGroupToServerLibraryHandler.Result.ReleaseGroupDoesNotExist =>
                new AddReleaseGroupToServerLibraryResult.AddReleaseGroupToServerLibraryReleaseGroupDoesNotExist(
                    "ReleaseGroup does not exist in MusicBrainz!"
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
    public record AddReleaseGroupToServerLibrarySuccess(User.User Viewer)
        : AddReleaseGroupToServerLibraryResult;

    public record AddReleaseGroupToServerLibraryReleaseGroupAlreadyAdded(string Message)
        : AddReleaseGroupToServerLibraryResult;

    public record AddReleaseGroupToServerLibraryReleaseGroupDoesNotExist(string Message)
        : AddReleaseGroupToServerLibraryResult;

    public record AddReleaseGroupToServerLibraryUnknownError(string Message)
        : AddReleaseGroupToServerLibraryResult;
}
