using MusicGQL.Features.Artists;
using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ServerLibrary.Cache;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class RefreshArtistMetaDataMutation
{
    public async Task<RefreshArtistMetaDataResult> RefreshArtistMetaData(
        RefreshArtistMetaDataInput input,
        [Service] ServerLibraryCache cache,
        [Service] LastFmEnrichmentService enrichment
    )
    {
        var artist = await cache.GetArtistByIdAsync(input.ArtistId);
        var mbId = artist?.JsonArtist.Connections?.MusicBrainzArtistId;
        if (artist == null || string.IsNullOrWhiteSpace(mbId))
        {
            return new RefreshArtistMetaDataError("Artist not found or missing MusicBrainz ID");
        }

        var dir = Path.Combine("./Library/", input.ArtistId);
        var res = await enrichment.EnrichArtistAsync(dir, mbId!);
        if (!res.Success)
        {
            return new RefreshArtistMetaDataError(res.ErrorMessage ?? "Unknown error");
        }

        await cache.UpdateCacheAsync();

        var artistAfterRefresh = await cache.GetArtistByIdAsync(input.ArtistId);

        if (artistAfterRefresh == null)
        {
            return new RefreshArtistMetaDataError("Could not find artist after refresh");
        }

        return new RefreshArtistMetaDataSuccess(new Artist(artistAfterRefresh));
    }
}

public record RefreshArtistMetaDataInput(string ArtistId);

[UnionType("RefreshArtistMetaDataResult")]
public abstract record RefreshArtistMetaDataResult;

public record RefreshArtistMetaDataSuccess(Artist Artist) : RefreshArtistMetaDataResult;

public record RefreshArtistMetaDataError(string Message) : RefreshArtistMetaDataResult;
