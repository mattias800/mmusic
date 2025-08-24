using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Share;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class GenerateArtistShareFilesMutation
{
    public async Task<GenerateArtistShareFilesResult> GenerateArtistShareFiles(
        GenerateArtistShareFilesInput input,
        [Service] ServerLibraryCache cache,
        [Service] ArtistShareManifestService shareService
    )
    {
        var artist = await cache.GetArtistByIdAsync(input.ArtistId);
        if (artist is null)
        {
            return new GenerateArtistShareFilesError("Artist not found");
        }

        var (tagFileName, manifestPath) = await shareService.GenerateForArtistAsync(input.ArtistId);
        return new GenerateArtistShareFilesSuccess(
            artist.Id,
            artist.Name,
            tagFileName,
            manifestPath
        );
    }
}

public record GenerateArtistShareFilesInput([ID] string ArtistId);

[UnionType]
public abstract record GenerateArtistShareFilesResult;

public record GenerateArtistShareFilesSuccess(
    string ArtistId,
    string ArtistName,
    string TagFileName,
    string ManifestPath
) : GenerateArtistShareFilesResult;

public record GenerateArtistShareFilesError(string Message) : GenerateArtistShareFilesResult;
