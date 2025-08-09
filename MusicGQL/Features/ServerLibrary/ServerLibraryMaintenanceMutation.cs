using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ServerLibrary.Cache;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary;

[ExtendObjectType(typeof(Types.Mutation))]
public class ServerLibraryMaintenanceMutation
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

public class RefreshArtistLastFmInput
{
    public string ArtistId { get; set; } = string.Empty;
}

public class RefreshArtistLastFmResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

    // Keep all server library maintenance mutations in this class
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
            return new RefreshArtistLastFmResult
            {
                Success = false,
                ErrorMessage = "Artist not found or missing MusicBrainz ID",
            };
        }

        var dir = Path.Combine("./Library/", input.ArtistId);
        var res = await enrichment.EnrichArtistAsync(dir, mbId!);
        if (!res.Success)
        {
            return new RefreshArtistLastFmResult
            {
                Success = false,
                ErrorMessage = res.ErrorMessage,
            };
        }

        await cache.UpdateCacheAsync();
        return new RefreshArtistLastFmResult { Success = true };
    }

    // 1) Reimport an album: delete metadata and cover, keep audio, reimport only this album
    [GraphQLName("reimportRelease")]
    public async Task<MaintenanceActionResult> ReimportRelease(
        [Service] ServerLibraryCache cache,
        [Service] MusicGQL.Features.Import.Services.MusicBrainzImportService mbImport,
        [Service] LibraryReleaseImportService releaseImporter,
        string artistId,
        string releaseFolderName
    )
    {
        var release = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);
        if (release == null)
        {
            return new MaintenanceActionResult { Success = false, ErrorMessage = "Release not found" };
        }

        // Delete metadata: release.json and cover art file referenced by it
        var releaseJsonPath = Path.Combine(release.ReleasePath, "release.json");
        string? coverArtRel = release.JsonRelease.CoverArt;
        string? coverArtAbs = null;
        if (!string.IsNullOrWhiteSpace(coverArtRel))
        {
            var rel = coverArtRel.StartsWith("./") ? coverArtRel[2..] : coverArtRel;
            coverArtAbs = Path.Combine(release.ReleasePath, rel);
        }

        try
        {
            if (File.Exists(releaseJsonPath))
            {
                File.Delete(releaseJsonPath);
            }
            if (!string.IsNullOrWhiteSpace(coverArtAbs) && File.Exists(coverArtAbs))
            {
                File.Delete(coverArtAbs);
            }
        }
        catch (Exception ex)
        {
            return new MaintenanceActionResult { Success = false, ErrorMessage = $"Failed deleting metadata: {ex.Message}" };
        }

        // Find release group to import using MusicBrainz (by title under artist MBID)
        var artist = await cache.GetArtistByIdAsync(artistId);
        var mbArtistId = artist?.JsonArtist.Connections?.MusicBrainzArtistId;
        if (string.IsNullOrWhiteSpace(mbArtistId))
        {
            return new MaintenanceActionResult { Success = false, ErrorMessage = "Artist missing MusicBrainz ID" };
        }

        var rgs = await mbImport.GetArtistReleaseGroupsAsync(mbArtistId!);
        var match = rgs.FirstOrDefault(rg => string.Equals(rg.Title, release.Title, StringComparison.OrdinalIgnoreCase));
        if (match == null)
        {
            return new MaintenanceActionResult { Success = false, ErrorMessage = "Could not match release group by title" };
        }

        var importResult = await releaseImporter.ImportReleaseGroupAsync(match, Path.GetDirectoryName(release.ReleasePath) ?? Path.Combine("./Library", artistId), artistId);
        await cache.UpdateCacheAsync();
        return new MaintenanceActionResult { Success = importResult.Success, ErrorMessage = importResult.ErrorMessage };
    }

    // 2) Redownload an album: delete audio, clear audio references in JSON, reload cache
    [GraphQLName("redownloadRelease")]
    public async Task<MaintenanceActionResult> RedownloadRelease(
        [Service] ServerLibraryCache cache,
        string artistId,
        string releaseFolderName
    )
    {
        var release = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);
        if (release == null)
        {
            return new MaintenanceActionResult { Success = false, ErrorMessage = "Release not found" };
        }

        var dir = release.ReleasePath;
        try
        {
            if (Directory.Exists(dir))
            {
                var audioExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".mp3", ".flac", ".wav", ".m4a", ".ogg" };
                foreach (var file in Directory.GetFiles(dir))
                {
                    var ext = Path.GetExtension(file);
                    if (audioExts.Contains(ext))
                    {
                        File.Delete(file);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return new MaintenanceActionResult { Success = false, ErrorMessage = $"Failed deleting audio files: {ex.Message}" };
        }

        // Update release.json to clear audio references
        var releaseJsonPath = Path.Combine(dir, "release.json");
        try
        {
            if (File.Exists(releaseJsonPath))
            {
                var jsonText = await File.ReadAllTextAsync(releaseJsonPath);
                var json = System.Text.Json.JsonSerializer.Deserialize<MusicGQL.Features.ServerLibrary.Json.JsonRelease>(jsonText, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase) }
                });
                if (json?.Tracks != null)
                {
                    foreach (var t in json.Tracks)
                    {
                        t.AudioFilePath = null;
                    }
                    var updated = System.Text.Json.JsonSerializer.Serialize(json, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                        WriteIndented = true,
                        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase) }
                    });
                    await File.WriteAllTextAsync(releaseJsonPath, updated);
                }
            }
        }
        catch (Exception ex)
        {
            return new MaintenanceActionResult { Success = false, ErrorMessage = $"Failed updating release.json: {ex.Message}" };
        }

        await cache.UpdateCacheAsync();
        return new MaintenanceActionResult { Success = true };
    }
}

public class MaintenanceActionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ScanLibraryResult
{
    public bool Success { get; set; }
    public int ArtistsCreated { get; set; }
    public int ReleasesCreated { get; set; }
    public List<string> Notes { get; set; } = [];
    public string? ErrorMessage { get; set; }
}
