using System.Security.Claims;
using MusicGQL.Features.Artists;
using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Types;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class ImportArtistMutation
{
    /// <summary>
    /// Imports an artist from MusicBrainz using the new background approach.
    /// Creates basic artist structure immediately and processes full import in background.
    /// </summary>
    public async Task<ImportArtistResult> ImportArtist(
        [Service] LibraryImportService importService,
        [Service] ServerLibraryCache cache,
        [Service] ServerLibrary.Share.ArtistShareManifestService shareService,
        [Service] ArtistImportBackgroundQueueService backgroundQueue,
        [Service] MusicBrainzImportService mbImport,
        [Service] IImportExecutor importExecutor,
        [Service] ServerSettings.ServerSettingsAccessor serverSettingsAccessor,
        ImportArtistInput input,
        [Service] IHttpContextAccessor httpContextAccessor
    )
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out _))
        {
            return new ImportArtistError("User not authenticated or invalid user ID.");
        }

        try
        {
            // Step 1: Quick validation and basic artist creation
            var mbArtist = await mbImport.GetArtistByIdAsync(input.MusicBrainzArtistId);
            if (mbArtist == null)
            {
                return new ImportArtistError("Artist not found on MusicBrainz.");
            }

            // Step 2: Create basic artist folder and artist.json
            var artistFolderName = SanitizeFolderName(mbArtist.Name);
            var libraryPath = await GetLibraryPathAsync(serverSettingsAccessor);
            var artistFolderPath = Path.Combine(libraryPath, artistFolderName);
            
            if (!Directory.Exists(artistFolderPath))
            {
                Directory.CreateDirectory(artistFolderPath);
            }

            // Step 3: Create minimal artist.json with basic info
            var artistJsonPath = Path.Combine(artistFolderPath, "artist.json");
            var basicArtistJson = new JsonArtist
            {
                Id = artistFolderName,
                Name = mbArtist.Name,
                Connections = new JsonArtistServiceConnections 
                { 
                    MusicBrainzArtistId = input.MusicBrainzArtistId 
                }
            };

            // Write basic artist.json
            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };
            var jsonText = System.Text.Json.JsonSerializer.Serialize(basicArtistJson, jsonOptions);
            await File.WriteAllTextAsync(artistJsonPath, jsonText);

            // Step 4: Update cache to include the new artist
            await cache.UpdateCacheAsync();
            
            // Step 5: Get the cached artist for return
            var cached = await cache.GetArtistByIdAsync(artistFolderName);
            if (cached is null)
            {
                return new ImportArtistError("Artist was created but not found in cache.");
            }

            // Step 6: Enqueue full import for background processing
            var backgroundJob = new ArtistImportBackgroundJob(
                ArtistId: artistFolderName,
                ArtistName: mbArtist.Name,
                MusicBrainzId: input.MusicBrainzArtistId,
                ArtistPath: artistFolderPath
            );
            
            backgroundQueue.Enqueue(backgroundJob);

            // Step 7: Generate share files (non-blocking)
            try 
            { 
                await shareService.GenerateForArtistAsync(artistFolderName); 
            } 
            catch 
            { 
                // Non-critical, continue 
            }

            return new ImportArtistSuccess(new Artist(cached));
        }
        catch (Exception ex)
        {
            return new ImportArtistError($"Failed to import artist: {ex.Message}");
        }
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

    private static string SanitizeFolderName(string artistName)
    {
        if (string.IsNullOrWhiteSpace(artistName))
            return "Unknown Artist";

        // Remove or replace invalid characters for folder names
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = artistName;
        
        foreach (var invalidChar in invalidChars)
        {
            sanitized = sanitized.Replace(invalidChar, '_');
        }

        // Trim and ensure it's not empty
        sanitized = sanitized.Trim('_', ' ', '.');
        if (string.IsNullOrWhiteSpace(sanitized))
            return "Unknown Artist";

        return sanitized;
    }

    private static async Task<string> GetLibraryPathAsync(ServerSettings.ServerSettingsAccessor serverSettingsAccessor)
    {
        try
        {
            var settings = await serverSettingsAccessor.GetAsync();
            return settings.LibraryPath;
        }
        catch
        {
            return string.Empty;
        }
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
