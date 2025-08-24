using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class RefreshAllReleasesForArtistMutation
{
    public async Task<RefreshAllReleasesForArtistResult> RefreshAllReleasesForArtist(
        ServerLibraryCache cache,
        ArtistImportQueue.Services.ArtistImportQueueService artistQueue,
        string artistId
    )
    {
        var artist = await cache.GetArtistByIdAsync(artistId);
        if (artist == null)
        {
            return new RefreshAllReleasesForArtistError("Artist not found");
        }

        var items = artist.Releases.Select(r => new ArtistImportQueue.ArtistImportQueueItem(
            artist.Name,
            null
        )
        {
            JobKind = ArtistImportQueue.ArtistImportJobKind.RefreshReleaseMetadata,
            LocalArtistId = artistId,
            ReleaseFolderName = r.FolderName,
        });
        artistQueue.Enqueue(items);
        return new RefreshAllReleasesForArtistSuccess(artistId, artist.Releases.Count);
    }
}

[UnionType("RefreshAllReleasesForArtistResult")]
public abstract record RefreshAllReleasesForArtistResult;

public record RefreshAllReleasesForArtistSuccess(string ArtistId, int RefreshedCount)
    : RefreshAllReleasesForArtistResult;

public record RefreshAllReleasesForArtistError(string Message) : RefreshAllReleasesForArtistResult;
