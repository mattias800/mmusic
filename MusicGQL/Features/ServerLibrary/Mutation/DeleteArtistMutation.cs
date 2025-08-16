using MusicGQL.Features.ServerLibrary.Cache;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class DeleteArtistMutation
{
    public async Task<DeleteArtistResult> DeleteArtist(
        DeleteArtistInput input,
        [Service] Services.ArtistDeletionService deletionService
    )
    {
        var (success, error) = await deletionService.DeleteArtistCompletelyAsync(input.ArtistId);
        if (!success)
        {
            return new DeleteArtistError(error ?? "Unknown error");
        }
        return new DeleteArtistSuccess(input.ArtistId);
    }
}

public record DeleteArtistInput([ID] string ArtistId);

[UnionType("DeleteArtistResult")]
public abstract record DeleteArtistResult;

public record DeleteArtistSuccess([ID] string DeletedArtistId) : DeleteArtistResult;

public record DeleteArtistError(string Message) : DeleteArtistResult;


