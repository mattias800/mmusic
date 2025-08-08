using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ServerLibrary.Cache;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public partial class ServerLibraryMaintenanceMutation
{
    public async Task<ScanLibraryResult> ScanLibraryForMissingJson(
        [Service] LibraryMaintenanceCoordinator coordinator
    )
    {
        var scan = await coordinator.RunAsync();
        return new ScanLibraryResult
        {
            Success = scan.Success,
            ArtistsCreated = scan.ArtistsCreated,
            ReleasesCreated = scan.ReleasesCreated,
            Notes = scan.Notes,
            ErrorMessage = scan.ErrorMessage,
        };
    }
}

public class RefreshArtistLastFmInput
{
    public string ArtistId { get; set; } = string.Empty;
}

public class RefreshArtistLastFmResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public partial class ServerLibraryMaintenanceMutation
{
    [GraphQLName("refreshArtistLastFm")]
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
            return new RefreshArtistLastFmResult { Success = false, ErrorMessage = "Artist not found or missing MusicBrainz ID" };
        }

        var dir = Path.Combine("./Library/", input.ArtistId);
        var res = await enrichment.EnrichArtistAsync(dir, mbId!);
        if (!res.Success)
        {
            return new RefreshArtistLastFmResult { Success = false, ErrorMessage = res.ErrorMessage };
        }

        await cache.UpdateCacheAsync();
        return new RefreshArtistLastFmResult { Success = true };
    }
}

public class ScanLibraryResult
{
    public bool Success { get; set; }
    public int ArtistsCreated { get; set; }
    public int ReleasesCreated { get; set; }
    public List<string> Notes { get; set; } = [];
    public string? ErrorMessage { get; set; }
}
