using MusicGQL.Features.ServerLibrary.Artist.Handlers;
using MusicGQL.Types;

namespace MusicGQL.Features.ServerLibrary.Artist.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class AddArtistToServerLibraryMutation
{
    public async Task<AddArtistToServerLibraryResult> AddArtistToServerLibrary(
        MarkArtistAsAddedToServerLibraryHandler markArtistAsAddedToServerLibraryHandler,
        MarkArtistReleaseGroupsAsAddedToServerLibraryHandler markArtistReleaseGroupsAsAddedToServerLibraryHandler,
        MissingMetaDataProcessingService missingMetaDataProcessingService,
        AddArtistToServerLibraryInput input
    )
    {
        return await markArtistAsAddedToServerLibraryHandler.Handle(new(input.ArtistId)) switch
        {
            MarkArtistAsAddedToServerLibraryHandler.Result.Success => await HandleSuccess(
                markArtistReleaseGroupsAsAddedToServerLibraryHandler,
                missingMetaDataProcessingService,
                input.ArtistId,
                new AddArtistToServerLibraryResult.AddArtistToServerLibrarySuccess(true)
            ),
            MarkArtistAsAddedToServerLibraryHandler.Result.AlreadyAdded => await HandleSuccess(
                markArtistReleaseGroupsAsAddedToServerLibraryHandler,
                missingMetaDataProcessingService,
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
        MissingMetaDataProcessingService missingMetaDataProcessingService,
        string artistId,
        AddArtistToServerLibraryResult success
    )
    {
        await markArtistReleaseGroupsAsAddedToServerLibraryHandler.Handle(new(artistId));
        missingMetaDataProcessingService.ProcessMissingMetaData();
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
