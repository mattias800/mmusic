using System.Text.Json;
using System.Text.Json.Serialization;
using Hqub.Lastfm;
using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ServerSettings;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import;

/// <summary>
/// Main service for importing artists and releases from external sources into the local music library
/// </summary>
public class LibraryImportService(
    MusicBrainzImportService musicBrainzService,
    SpotifyImportService spotifyService,
    FanArtDownloadService fanArtService,
    LastfmClient lastfmClient,
    ServerLibraryCache cache,
    LibraryReleaseImportService releaseImporter,
    IImportExecutor importExecutor,
    LastFmEnrichmentService enrichmentService,
    MusicGQL.Features.ServerSettings.ServerSettingsAccessor serverSettingsAccessor,
    ILogger<LibraryImportService> logger
)
{
    private async Task<string> GetLibraryPathAsync()
    {
        try
        {
            var s = await serverSettingsAccessor.GetAsync();
            return s.LibraryPath;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Imports an artist by searching MusicBrainz and Spotify, downloading photos, and creating artist.json
    /// </summary>
    /// <param name="artistName">Name of the artist to import</param>
    /// <returns>Import result with success status and details</returns>
    public async Task<ArtistImportResult> ImportArtistAsync(string artistName)
    {
        // For backward compatibility, delegate to the new ID-based method by first resolving the MBID
        var mbResults = await musicBrainzService.SearchArtistsAsync(artistName);
        var mbArtist = mbResults.FirstOrDefault();
        if (mbArtist is null)
        {
            return new ArtistImportResult
            {
                ArtistName = artistName,
                ErrorMessage = "Artist not found on MusicBrainz",
            };
        }
        return await ImportArtistByMusicBrainzIdAsync(mbArtist.Id);
    }

    /// <summary>
    /// Imports an artist directly by MusicBrainz artist ID, and also imports all eligible releases.
    /// This now delegates to the same executor used by the file-scanner flow so both paths are unified.
    /// </summary>
    /// <param name="musicBrainzArtistId">MusicBrainz artist ID (MBID)</param>
    public async Task<ArtistImportResult> ImportArtistByMusicBrainzIdAsync(
        string musicBrainzArtistId
    )
    {
        var result = new ArtistImportResult { MusicBrainzId = musicBrainzArtistId };
        var startTime = DateTime.UtcNow;

        try
        {
            logger.LogInformation("[ImportArtist] 🎤 Starting artist import for MBID: {MusicBrainzId}", musicBrainzArtistId);

            // 1) Resolve MB artist to get display name and canonicalize folder
            logger.LogInformation("[ImportArtist] 🔍 Step 1: Resolving MusicBrainz artist data for MBID: {MusicBrainzId}", musicBrainzArtistId);
            var mbArtist = await musicBrainzService.GetArtistByIdAsync(musicBrainzArtistId);
            if (mbArtist is null)
            {
                logger.LogError("[ImportArtist] ❌ Failed to find artist on MusicBrainz with MBID: {MusicBrainzId}", musicBrainzArtistId);
                result.ErrorMessage = "Artist not found on MusicBrainz";
                return result;
            }
            
            logger.LogInformation("[ImportArtist] ✅ Found MusicBrainz artist: '{ArtistName}' (MBID: {MusicBrainzId})", mbArtist.Name, musicBrainzArtistId);
            result.ArtistName = mbArtist.Name;

            var artistFolderName = SanitizeFolderName(mbArtist.Name);
            var artistFolderPath = Path.Combine(await GetLibraryPathAsync(), artistFolderName);
            
            logger.LogInformation("[ImportArtist] 📁 Step 2: Creating artist folder structure at: {ArtistFolderPath}", artistFolderPath);
            if (!Directory.Exists(artistFolderPath))
            {
                Directory.CreateDirectory(artistFolderPath);
                logger.LogInformation("[ImportArtist] ✅ Created new artist directory: {ArtistFolderPath}", artistFolderPath);
            }
            else
            {
                logger.LogInformation("[ImportArtist] ℹ️ Artist directory already exists: {ArtistFolderPath}", artistFolderPath);
            }
            result.ArtistFolderPath = artistFolderPath;

            // 2) Use the same executor as the file-scanner to create/enrich artist.json (photos, connections, top tracks)
            logger.LogInformation("[ImportArtist] 🎨 Step 3: Importing/enriching artist metadata and photos");
            var artistImportStart = DateTime.UtcNow;
            await importExecutor.ImportOrEnrichArtistAsync(
                artistFolderPath,
                mbArtist.Id,
                mbArtist.Name
            );
            var artistImportDuration = DateTime.UtcNow - artistImportStart;
            logger.LogInformation("[ImportArtist] ✅ Artist metadata import completed in {DurationMs}ms", artistImportDuration.TotalMilliseconds);

            // 3) Import all eligible release groups (albums, EPs, singles)
            logger.LogInformation("[ImportArtist] 💿 Step 4: Importing eligible release groups for artist '{ArtistName}'", mbArtist.Name);
            var releasesImportStart = DateTime.UtcNow;
            var importedCount = await importExecutor.ImportEligibleReleaseGroupsAsync(artistFolderPath, mbArtist.Id);
            var releasesImportDuration = DateTime.UtcNow - releasesImportStart;
            logger.LogInformation("[ImportArtist] ✅ Imported {ImportedCount} release groups in {DurationMs}ms", importedCount, releasesImportDuration.TotalMilliseconds);

            // 4) Enrich artist (map top tracks to releases, fill missing info)
            logger.LogInformation("[ImportArtist] 🔗 Step 5: Enriching artist with additional metadata and track mapping");
            var enrichmentStart = DateTime.UtcNow;
            try
            {
                await enrichmentService.EnrichArtistAsync(artistFolderPath, mbArtist.Id);
                var enrichmentDuration = DateTime.UtcNow - enrichmentStart;
                logger.LogInformation("[ImportArtist] ✅ Artist enrichment completed in {DurationMs}ms", enrichmentDuration.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[ImportArtist] ⚠️ Artist enrichment failed, continuing without enrichment");
            }

            // 5) Update cache and load artist json for return
            logger.LogInformation("[ImportArtist] 🗄️ Step 6: Updating server library cache");
            var cacheUpdateStart = DateTime.UtcNow;
            await cache.UpdateCacheAsync();
            var cacheUpdateDuration = DateTime.UtcNow - cacheUpdateStart;
            logger.LogInformation("[ImportArtist] ✅ Cache update completed in {DurationMs}ms", cacheUpdateDuration.TotalMilliseconds);

            // Read back artist.json to include in result
            logger.LogInformation("[ImportArtist] 📖 Step 7: Reading final artist.json for result");
            try
            {
                var artistJsonText = await File.ReadAllTextAsync(
                    Path.Combine(artistFolderPath, "artist.json")
                );
                result.ArtistJson = JsonSerializer.Deserialize<JsonArtist>(
                    artistJsonText,
                    GetJsonOptions()
                );
            }
            catch { }

            result.Success = true;
            var totalDuration = DateTime.UtcNow - startTime;
            logger.LogInformation("[ImportArtist] 🎉 Successfully imported artist '{ArtistName}' in {TotalDurationMs}ms", mbArtist.Name, totalDuration.TotalMilliseconds);
            logger.LogInformation("[ImportArtist] 📊 Import Summary: Artist metadata: {ArtistMs}ms, Releases: {ReleasesMs}ms, Enrichment: {EnrichmentMs}ms, Cache: {CacheMs}ms", 
                artistImportDuration.TotalMilliseconds, releasesImportDuration.TotalMilliseconds, enrichmentStart - enrichmentStart, cacheUpdateDuration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            var totalDuration = DateTime.UtcNow - startTime;
            logger.LogError(ex, "[ImportArtist] ❌ Failed to import artist with MBID '{MusicBrainzId}' after {TotalDurationMs}ms", musicBrainzArtistId, totalDuration.TotalMilliseconds);
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Imports all releases for an artist from MusicBrainz
    /// </summary>
    /// <param name="artistId">Local artist ID (folder name)</param>
    /// <returns>Import result with success status and details</returns>
    public async Task<ReleaseImportResult> ImportArtistReleasesAsync(string artistId)
    {
        var result = new ReleaseImportResult { ArtistId = artistId };
        var startTime = DateTime.UtcNow;

        try
        {
            logger.LogInformation("[ImportReleases] 💿 Starting release import for artist: {ArtistId}", artistId);

            // 1. Get artist info to find MusicBrainz ID
            logger.LogInformation("[ImportReleases] 🔍 Step 1: Retrieving artist information from cache");
            var artist = await cache.GetArtistByIdAsync(artistId);
            if (artist?.JsonArtist.Connections?.MusicBrainzArtistId == null)
            {
                logger.LogError("[ImportReleases] ❌ Artist '{ArtistId}' not found in cache or missing MusicBrainz ID", artistId);
                result.ErrorMessage = "Artist not found or missing MusicBrainz ID";
                return result;
            }

            var mbArtistId = artist.JsonArtist.Connections.MusicBrainzArtistId;
            var artistFolderPath = Path.Combine(await GetLibraryPathAsync(), artistId);
            logger.LogInformation("[ImportReleases] ✅ Found artist '{ArtistName}' with MBID: {MusicBrainzId}", artist.JsonArtist.Name, mbArtistId);

            // 2. Get release groups from MusicBrainz
            logger.LogInformation("[ImportReleases] 🔍 Step 2: Fetching release groups from MusicBrainz for artist '{ArtistName}'", artist.JsonArtist.Name);
            var releaseGroupsStart = DateTime.UtcNow;
            var releaseGroups = await musicBrainzService.GetArtistReleaseGroupsAsync(mbArtistId);
            var releaseGroupsDuration = DateTime.UtcNow - releaseGroupsStart;

            logger.LogInformation("[ImportReleases] 📀 Found {ReleaseGroupCount} release groups in {DurationMs}ms", releaseGroups.Count, releaseGroupsDuration.TotalMilliseconds);

            // 3. Import each release group
            logger.LogInformation("[ImportReleases] 🚀 Step 3: Starting import of {ReleaseGroupCount} release groups", releaseGroups.Count);
            var importStart = DateTime.UtcNow;
            var successfulImports = 0;
            var failedImports = 0;
            
            foreach (var releaseGroup in releaseGroups)
            {
                try
                {
                    logger.LogInformation("[ImportReleases] 📀 Importing release group: '{Title}' (Type: {PrimaryType})", releaseGroup.Title, releaseGroup.PrimaryType);
                    var singleReleaseStart = DateTime.UtcNow;
                    
                    var releaseResult = await ImportReleaseGroupAsync(
                        releaseGroup,
                        artistFolderPath,
                        artistId
                    );
                    result.ImportedReleases.Add(releaseResult);

                    var singleReleaseDuration = DateTime.UtcNow - singleReleaseStart;
                    if (releaseResult.Success)
                    {
                        successfulImports++;
                        logger.LogInformation("[ImportReleases] ✅ Successfully imported release '{Title}' in {DurationMs}ms", releaseGroup.Title, singleReleaseDuration.TotalMilliseconds);
                    }
                    else
                    {
                        failedImports++;
                        logger.LogWarning("[ImportReleases] ⚠️ Failed to import release '{Title}' after {DurationMs}ms: {ErrorMessage}", 
                            releaseGroup.Title, singleReleaseDuration.TotalMilliseconds, releaseResult.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    failedImports++;
                    logger.LogError(ex, "[ImportReleases] ❌ Exception while importing release group '{Title}'", releaseGroup.Title);
                    result.ImportedReleases.Add(new SingleReleaseImportResult
                    {
                        Success = false,
                        Title = releaseGroup.Title,
                        ErrorMessage = ex.Message
                    });
                }
            }

            var totalImportDuration = DateTime.UtcNow - importStart;
            var totalDuration = DateTime.UtcNow - startTime;
            
            result.Success = true;
            // Note: TotalReleases, SuccessfulReleases, and FailedReleases are computed properties

            logger.LogInformation("[ImportReleases] 🎉 Release import completed in {TotalDurationMs}ms", totalDuration.TotalMilliseconds);
            logger.LogInformation("[ImportReleases] 📊 Import Summary: {Successful}/{Total} successful, {Failed} failed", 
                result.SuccessfulReleases, result.TotalReleases, result.FailedReleases);
            logger.LogInformation("[ImportReleases] ⏱️ Timing: Release groups fetch: {GroupsMs}ms, Individual imports: {ImportsMs}ms", 
                releaseGroupsDuration.TotalMilliseconds, totalImportDuration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            var totalDuration = DateTime.UtcNow - startTime;
            logger.LogError(ex, "[ImportReleases] ❌ Failed to import releases for artist '{ArtistId}' after {TotalDurationMs}ms", artistId, totalDuration.TotalMilliseconds);
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Imports a single release group with its releases and tracks
    /// </summary>
    private async Task<SingleReleaseImportResult> ImportReleaseGroupAsync(
        MusicBrainzReleaseGroupResult releaseGroup,
        string artistFolderPath,
        string artistId
    )
    {
        // Delegate to dedicated release import service for single-responsibility and reuse
        return await releaseImporter.ImportReleaseGroupAsync(
            releaseGroup,
            artistFolderPath,
            artistId
        );
    }

    /// <summary>
    /// Sanitizes a string to be safe for use as a folder name
    /// </summary>
    private static string SanitizeFolderName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join(
            "",
            name.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)
        );
        return sanitized.Trim();
    }

    /// <summary>
    /// Gets JSON serializer options consistent with the rest of the application
    /// </summary>
    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };
    }
}

/// <summary>
/// Result of importing an artist
/// </summary>
public class ArtistImportResult
{
    public bool Success { get; set; }
    public string ArtistName { get; set; } = string.Empty;
    public string? MusicBrainzId { get; set; }
    public string? SpotifyId { get; set; }
    public string? ArtistFolderPath { get; set; }
    public FanArtDownloadResult? DownloadedPhotos { get; set; }
    public JsonArtist? ArtistJson { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of importing releases for an artist
/// </summary>
public class ReleaseImportResult
{
    public bool Success { get; set; }
    public string ArtistId { get; set; } = string.Empty;
    public List<SingleReleaseImportResult> ImportedReleases { get; set; } = [];
    public string? ErrorMessage { get; set; }
    public int TotalReleases => ImportedReleases.Count;
    public int SuccessfulReleases => ImportedReleases.Count(r => r.Success);
    public int FailedReleases => ImportedReleases.Count(r => !r.Success);
}

/// <summary>
/// Result of importing a single release
/// </summary>
public class SingleReleaseImportResult
{
    public bool Success { get; set; }
    public string ReleaseGroupId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? ReleaseFolderPath { get; set; }
    public JsonRelease? ReleaseJson { get; set; }
    public string? ErrorMessage { get; set; }
}
