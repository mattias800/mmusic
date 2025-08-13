using System.Security.Claims;
using MusicGQL.Features.Artists;
using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Types;

namespace MusicGQL.Features.Import.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class ImportArtistMutation
{
    /// <summary>
    /// Imports an artist from MusicBrainz and Spotify, downloads photos from fanart.tv
    /// </summary>
    public async Task<ImportArtistResult> ImportArtist(
        [Service] LibraryImportService importService,
        [Service] ServerLibraryCache cache,
        [Service] MusicGQL.Features.ServerLibrary.Share.ArtistShareManifestService shareService,
        ImportArtistInput input,
        [Service] IHttpContextAccessor httpContextAccessor
    )
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out _))
        {
            return new ImportArtistError("User not authenticated or invalid user ID.");
        }

        var result = await importService.ImportArtistByMusicBrainzIdAsync(
            input.MusicBrainzArtistId
        );

        if (!result.Success)
        {
            return new ImportArtistError(result.ErrorMessage ?? "Unknown error importing artist.");
        }

        var artistId = result.ArtistJson?.Id;
        if (string.IsNullOrEmpty(artistId))
        {
            return new ImportArtistError("Artist was imported but no artist ID was produced.");
        }

        var cached = await cache.GetArtistByIdAsync(artistId);
        if (cached is null)
        {
            return new ImportArtistError("Artist was imported but not found in cache.");
        }

        try { await shareService.GenerateForArtistAsync(artistId); } catch { }

        return new ImportArtistSuccess(new Artist(cached));
    }

    /// <summary>
    /// Imports all releases for an artist from MusicBrainz, downloads cover art
    /// </summary>
    /// <param name="artistId">Local artist ID (folder name)</param>
    /// <param name="importService">Library import service</param>
    /// <returns>Import result</returns>
    public async Task<ImportReleasesResult> ImportArtistReleases(
        string artistId,
        [Service] LibraryImportService importService
    )
    {
        var result = await importService.ImportArtistReleasesAsync(artistId);

        return new ImportReleasesResult
        {
            Success = result.Success,
            ArtistId = result.ArtistId,
            TotalReleases = result.ImportedReleases.Count,
            SuccessfulReleases = result.ImportedReleases.Count(r => r.Success),
            FailedReleases = result.ImportedReleases.Count(r => !r.Success),
            ImportedReleases = result
                .ImportedReleases.Select(r => new ImportedRelease
                {
                    Success = r.Success,
                    Title = r.Title,
                    ReleaseGroupId = r.ReleaseGroupId,
                    ErrorMessage = r.ErrorMessage,
                })
                .ToList(),
            ErrorMessage = result.ErrorMessage,
        };
    }
}

public record ImportArtistInput(string MusicBrainzArtistId);

[UnionType]
public abstract record ImportArtistResult;

public record ImportArtistSuccess(Artist Artist) : ImportArtistResult;

public record ImportArtistError(string Message) : ImportArtistResult;

/// <summary>
/// Result of importing releases for an artist
/// </summary>
public class ImportReleasesResult
{
    public bool Success { get; set; }
    public string ArtistId { get; set; } = string.Empty;
    public int TotalReleases { get; set; }
    public int SuccessfulReleases { get; set; }
    public int FailedReleases { get; set; }
    public List<ImportedRelease> ImportedReleases { get; set; } = [];
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Information about a single imported release
/// </summary>
public class ImportedRelease
{
    public bool Success { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ReleaseGroupId { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
