using MusicGQL.Features.ServerLibrary.Artist.Handlers;
using MusicGQL.Types;

namespace MusicGQL.Features.ServerLibrary.Artist.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class AddArtistToServerLibraryMutation
{
    public async Task<AddArtistToServerLibraryResult> AddArtistToServerLibrary(
        [Service] AddArtistToServerLibraryHandler handler,
        AddArtistToServerLibraryInput input
    )
    {
        return await handler.Handle(new(input.ArtistId)) switch
        {
            AddArtistToServerLibraryHandler.Result.Success =>
                new AddArtistToServerLibraryResult.AddArtistToServerLibrarySuccess(
                    new ArtistServerAvailability(input.ArtistId)
                ),
            AddArtistToServerLibraryHandler.Result.AlreadyAdded =>
                new AddArtistToServerLibraryResult.AddArtistToServerLibraryArtistAlreadyAdded(
                    "Artist already added!"
                ),
            AddArtistToServerLibraryHandler.Result.ArtistDoesNotExist =>
                new AddArtistToServerLibraryResult.AddArtistToServerLibraryArtistDoesNotExist(
                    "Artist does not exist in MusicBrainz!"
                ),
            _ => new AddArtistToServerLibraryResult.AddArtistToServerLibraryUnknownError(
                "Unhandled result."
            ),
        };
    }
};

public record AddArtistToServerLibraryInput([property: ID] string ArtistId);

[UnionType("AddArtistToServerLibraryResult")]
public abstract record AddArtistToServerLibraryResult
{
    public record AddArtistToServerLibrarySuccess(ArtistServerAvailability ServerAvailability)
        : AddArtistToServerLibraryResult;

    public record AddArtistToServerLibraryArtistAlreadyAdded(string Message)
        : AddArtistToServerLibraryResult;

    public record AddArtistToServerLibraryArtistDoesNotExist(string Message)
        : AddArtistToServerLibraryResult;

    public record AddArtistToServerLibraryUnknownError(string Message)
        : AddArtistToServerLibraryResult;
}
