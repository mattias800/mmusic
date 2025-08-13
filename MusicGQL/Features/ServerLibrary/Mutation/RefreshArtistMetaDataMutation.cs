using MusicGQL.Features.Artists;
using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerSettings;
using Microsoft.Extensions.Logging;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class RefreshArtistMetaDataMutation
{
    public async Task<RefreshArtistMetaDataResult> RefreshArtistMetaData(
        ServerLibraryCache cache,
        LastFmEnrichmentService enrichment,
        IImportExecutor importExecutor,
        ILogger<RefreshArtistMetaDataMutation> logger,
        HotChocolate.Subscriptions.ITopicEventSender eventSender,
        [Service] MusicGQL.Features.ServerLibrary.Share.ArtistShareManifestService shareService,
        [Service] ServerSettingsAccessor serverSettingsAccessor,
        RefreshArtistMetaDataInput input
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
                var exact = matches.FirstOrDefault(a =>
                    string.Equals(a.Name, input.ArtistId, StringComparison.OrdinalIgnoreCase));
                if (exact != null)
                {
                    logger.LogInformation(
                        "[RefreshArtistMetaData] Artist not found by id. Using exact name match -> id='{Id}'",
                        exact.Id);
                    artist = exact;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[RefreshArtistMetaData] Error during name fallback for '{ArtistId}'",
                    input.ArtistId);
            }
        }

        var mbId = artist?.JsonArtist.Connections?.MusicBrainzArtistId;
        if (artist == null || string.IsNullOrWhiteSpace(mbId))
        {
            return new RefreshArtistMetaDataError("The artist you supplied could not be found");
        }

        var effectiveArtistId = artist.Id;
        var lib = await serverSettingsAccessor.GetAsync();
        var dir = Path.Combine(lib.LibraryPath, effectiveArtistId);
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
            logger.LogWarning(ex,
                "[RefreshArtistMetaData] Import/enrich executor failed; continuing to Last.fm enrichment");
        }

        // Backfill any eligible releases (covers previous runs where writes were blocked)
        try
        {
            await importExecutor.ImportEligibleReleaseGroupsAsync(dir, mbId!);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "[RefreshArtistMetaData] ImportEligibleReleaseGroupsAsync failed; continuing to enrichment");
        }

        var res = await enrichment.EnrichArtistAsync(dir, mbId!);
        if (!res.Success)
        {
            logger.LogWarning("[RefreshArtistMetaData] Enrichment failed for artistId='{ArtistId}': {Error}",
                effectiveArtistId, res.ErrorMessage ?? "Unknown error");
            return new RefreshArtistMetaDataError(res.ErrorMessage ?? "Unknown error");
        }

        logger.LogInformation(
            "[RefreshArtistMetaData] Enrichment complete for artistId='{ArtistId}'. Updating cache...",
            effectiveArtistId);
        await cache.UpdateCacheAsync();

        // After updating cache, generate share files (manifest + tag)
        try { await shareService.GenerateForArtistAsync(effectiveArtistId); } catch { }

        var artistAfterRefresh = await cache.GetArtistByIdAsync(effectiveArtistId);

        if (artistAfterRefresh == null)
        {
            logger.LogError("[RefreshArtistMetaData] Artist not found in cache after refresh for id='{ArtistId}'",
                effectiveArtistId);
            return new RefreshArtistMetaDataError("Could not find artist after refresh");
        }

        // Publish artist-updated so UI can refresh in realtime
        try
        {
            await eventSender.SendAsync(
                Subscription.LibrarySubscription.LibraryArtistUpdatedTopic(artistAfterRefresh.Id),
                new Artist(artistAfterRefresh)
            );
        }
        catch
        {
        }

        logger.LogInformation("[RefreshArtistMetaData] Success for artistId='{ArtistId}'", effectiveArtistId);
        return new RefreshArtistMetaDataSuccess(new Artist(artistAfterRefresh));
    }
}

public record RefreshArtistMetaDataInput(string ArtistId);

[UnionType("RefreshArtistMetaDataResult")]
public abstract record RefreshArtistMetaDataResult;

public record RefreshArtistMetaDataSuccess(Artist Artist) : RefreshArtistMetaDataResult;

public record RefreshArtistMetaDataError(string Message) : RefreshArtistMetaDataResult;