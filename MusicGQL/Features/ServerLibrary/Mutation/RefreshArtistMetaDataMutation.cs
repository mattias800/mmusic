using MusicGQL.Features.Artists;
using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ServerLibrary.Cache;
using Microsoft.Extensions.Logging;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class RefreshArtistMetaDataMutation
{
    public async Task<RefreshArtistMetaDataResult> RefreshArtistMetaData(
        RefreshArtistMetaDataInput input,
        [Service] ServerLibraryCache cache,
        [Service] LastFmEnrichmentService enrichment,
        [Service] MusicGQL.Features.Import.Services.IImportExecutor importExecutor,
        [Service] ILogger<RefreshArtistMetaDataMutation> logger,
        [Service] HotChocolate.Subscriptions.ITopicEventSender eventSender
    )
    {
        logger.LogInformation("[RefreshArtistMetaData] Requested refresh for artistId='{ArtistId}'", input.ArtistId);

        var artist = await cache.GetArtistByIdAsync(input.ArtistId);

        // Fallback: treat supplied id as a name for exact match
        if (artist == null)
        {
            try
            {
                var matches = await cache.SearchArtistsByNameAsync(input.ArtistId, 10);
                var exact = matches.FirstOrDefault(a => string.Equals(a.Name, input.ArtistId, StringComparison.OrdinalIgnoreCase));
                if (exact != null)
                {
                    logger.LogInformation("[RefreshArtistMetaData] Artist not found by id. Using exact name match -> id='{Id}'", exact.Id);
                    artist = exact;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[RefreshArtistMetaData] Error during name fallback for '{ArtistId}'", input.ArtistId);
            }
        }

        var mbId = artist?.JsonArtist.Connections?.MusicBrainzArtistId;
        if (artist == null || string.IsNullOrWhiteSpace(mbId))
        {
            return new RefreshArtistMetaDataError("The artist you supplied could not be found");
        }

        var effectiveArtistId = artist.Id;
        var dir = Path.Combine("./Library/", effectiveArtistId);
        logger.LogInformation(
            "[RefreshArtistMetaData] Starting enrichment for artistId='{ArtistId}', mbid='{MbId}', dir='{Dir}'",
            effectiveArtistId,
            mbId,
            Path.GetFullPath(dir)
        );
        // First, run the unified import/enrich executor to refresh assets (photos) and core connections
        try
        {
            await importExecutor.ImportOrEnrichArtistAsync(dir, mbId!, artist.Name);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[RefreshArtistMetaData] Import/enrich executor failed; continuing to Last.fm enrichment");
        }

        var res = await enrichment.EnrichArtistAsync(dir, mbId!);
        if (!res.Success)
        {
            logger.LogWarning("[RefreshArtistMetaData] Enrichment failed for artistId='{ArtistId}': {Error}", effectiveArtistId, res.ErrorMessage ?? "Unknown error");
            return new RefreshArtistMetaDataError(res.ErrorMessage ?? "Unknown error");
        }

        logger.LogInformation("[RefreshArtistMetaData] Enrichment complete for artistId='{ArtistId}'. Updating cache...", effectiveArtistId);
        await cache.UpdateCacheAsync();

        var artistAfterRefresh = await cache.GetArtistByIdAsync(effectiveArtistId);

        if (artistAfterRefresh == null)
        {
            logger.LogError("[RefreshArtistMetaData] Artist not found in cache after refresh for id='{ArtistId}'", effectiveArtistId);
            return new RefreshArtistMetaDataError("Could not find artist after refresh");
        }

        // Publish artist-updated so UI can refresh in realtime
        try
        {
            await eventSender.SendAsync(
                Features.ServerLibrary.Subscription.LibrarySubscription.LibraryArtistUpdatedTopic(artistAfterRefresh.Id),
                new Artist(artistAfterRefresh)
            );
        }
        catch { }

        logger.LogInformation("[RefreshArtistMetaData] Success for artistId='{ArtistId}'", effectiveArtistId);
        return new RefreshArtistMetaDataSuccess(new Artist(artistAfterRefresh));
    }
}

public record RefreshArtistMetaDataInput(string ArtistId);

[UnionType("RefreshArtistMetaDataResult")]
public abstract record RefreshArtistMetaDataResult;

public record RefreshArtistMetaDataSuccess(Artist Artist) : RefreshArtistMetaDataResult;

public record RefreshArtistMetaDataError(string Message) : RefreshArtistMetaDataResult;
