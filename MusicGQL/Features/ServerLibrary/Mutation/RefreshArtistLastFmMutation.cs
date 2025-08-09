using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ServerLibrary.Cache;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class RefreshArtistLastFmMutation
{
    public async Task<RefreshArtistLastFmResult> RefreshArtistLastFm(
        RefreshArtistLastFmInput input,
        [Service] ServerLibraryCache cache,
        [Service] LastFmEnrichmentService enrichment
    )
    {
        var artist = await cache.GetArtistByIdAsync(input.ArtistId);
        var mbId = artist?.JsonArtist.Connections?.MusicBrainzArtistId;
        if (artist == null || string.IsNullOrWhiteSpace(mbId))
        {
            return new RefreshArtistLastFmError("Artist not found or missing MusicBrainz ID");
        }

        var dir = Path.Combine("./Library/", input.ArtistId);
        var res = await enrichment.EnrichArtistAsync(dir, mbId!);
        if (!res.Success)
        {
            return new RefreshArtistLastFmError(res.ErrorMessage ?? "Unknown error");
        }

        await cache.UpdateCacheAsync();
        return new RefreshArtistLastFmSuccess(true);
    }
}

public record RefreshArtistLastFmInput(string ArtistId);

[UnionType("RefreshArtistLastFmResult")]
public abstract record RefreshArtistLastFmResult;

public record RefreshArtistLastFmSuccess(bool Success) : RefreshArtistLastFmResult;

public record RefreshArtistLastFmError(string Message) : RefreshArtistLastFmResult;

