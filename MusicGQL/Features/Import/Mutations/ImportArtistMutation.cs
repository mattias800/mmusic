using MusicGQL.Types;

namespace MusicGQL.Features.Import.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class ImportArtistMutation
{
    /// <summary>
    /// Imports an artist from MusicBrainz and Spotify, downloads photos from fanart.tv
    /// </summary>
    /// <param name="artistName">Name of the artist to import</param>
    /// <param name="importService">Library import service</param>
    /// <returns>Import result</returns>
    public async Task<ImportArtistResult> ImportArtist(
        string artistName,
        [Service] LibraryImportService importService
    )
    {
        var result = await importService.ImportArtistAsync(artistName);

        return new ImportArtistResult
        {
            Success = result.Success,
            ArtistName = result.ArtistName,
            ArtistId = result.ArtistJson?.Id,
            MusicBrainzId = result.MusicBrainzId,
            SpotifyId = result.SpotifyId,
            PhotosDownloaded = new PhotosDownloaded
            {
                Thumbs = result.DownloadedPhotos?.Thumbs.Count ?? 0,
                Backgrounds = result.DownloadedPhotos?.Backgrounds.Count ?? 0,
                Banners = result.DownloadedPhotos?.Banners.Count ?? 0,
                Logos = result.DownloadedPhotos?.Logos.Count ?? 0,
            },
            ErrorMessage = result.ErrorMessage,
        };
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

/// <summary>
/// Result of importing an artist
/// </summary>
public class ImportArtistResult
{
    public bool Success { get; set; }
    public string ArtistName { get; set; } = string.Empty;
    public string? ArtistId { get; set; }
    public string? MusicBrainzId { get; set; }
    public string? SpotifyId { get; set; }
    public PhotosDownloaded PhotosDownloaded { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Number of photos downloaded by type
/// </summary>
public class PhotosDownloaded
{
    public int Thumbs { get; set; }
    public int Backgrounds { get; set; }
    public int Banners { get; set; }
    public int Logos { get; set; }
}

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
