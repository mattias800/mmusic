using MusicGQL.Features.ServerLibrary.Artist.Handlers;
using MusicGQL.Features.ServerLibrary.Artist.Sagas;
using MusicGQL.Types;
using Rebus.Bus;

namespace MusicGQL.Features.ServerLibrary.Artist.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class AddArtistToServerLibraryMutation
{
    public async Task<AddArtistToServerLibraryResult> AddArtistToServerLibrary(
        [Service] AddArtistToServerLibraryHandler handler,
        [Service] IBus bus,
        AddArtistToServerLibraryInput input
    )
    {
        return await handler.Handle(new(input.ArtistId)) switch
        {
            AddArtistToServerLibraryHandler.Result.Success => await StartAddArtistSaga(
                bus,
                input.ArtistId,
                new AddArtistToServerLibraryResult.AddArtistToServerLibrarySuccess(
                    new ArtistServerAvailability(input.ArtistId)
                )
            ),
            AddArtistToServerLibraryHandler.Result.AlreadyAdded => await StartAddArtistSaga(
                bus,
                input.ArtistId,
                new AddArtistToServerLibraryResult.AddArtistToServerLibraryArtistAlreadyAdded(
                    "Artist already added!"
                )
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

    private async Task<AddArtistToServerLibraryResult> StartAddArtistSaga(
        IBus bus,
        string artistId,
        AddArtistToServerLibraryResult success
    )
    {
        await bus.Send(new AddArtistToServerLibrarySagaEvents.StartAddArtist(artistId));
        return success;
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
