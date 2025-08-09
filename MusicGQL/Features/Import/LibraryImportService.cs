using System.Text.Json;
using System.Text.Json.Serialization;
using Hqub.Lastfm;
using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import;

/// <summary>
/// Main service for importing artists and releases from external sources into the local library
/// </summary>
public class LibraryImportService(
    MusicBrainzImportService musicBrainzService,
    SpotifyImportService spotifyService,
    FanArtDownloadService fanArtService,
    LastfmClient lastfmClient,
    ServerLibraryCache cache,
    LibraryReleaseImportService releaseImporter
)
{
    private const string LibraryPath = "./Library/";

    /// <summary>
    /// Imports an artist by searching MusicBrainz and Spotify, downloading photos, and creating artist.json
    /// </summary>
    /// <param name="artistName">Name of the artist to import</param>
    /// <returns>Import result with success status and details</returns>
    public async Task<ArtistImportResult> ImportArtistAsync(string artistName)
    {
        var result = new ArtistImportResult { ArtistName = artistName };

        try
        {
            Console.WriteLine($"🎤 Importing artist: {artistName}");

            // 1. Search MusicBrainz for the artist
            Console.WriteLine("🔍 Searching MusicBrainz...");
            var mbResults = await musicBrainzService.SearchArtistsAsync(artistName);

            if (!mbResults.Any())
            {
                result.ErrorMessage = "Artist not found on MusicBrainz";
                return result;
            }

            // Take the first/best match from MusicBrainz
            var mbArtist = mbResults.First();
            result.MusicBrainzId = mbArtist.Id;
            Console.WriteLine($"✅ Found on MusicBrainz: {mbArtist.Name} ({mbArtist.Id})");

            // 2. Search Spotify for matching artist
            Console.WriteLine("🎵 Searching Spotify...");
            var spotifyArtist = await spotifyService.FindBestMatchAsync(artistName);
            if (spotifyArtist != null)
            {
                result.SpotifyId = spotifyArtist.Id;
                Console.WriteLine($"✅ Found on Spotify: {spotifyArtist.Name} ({spotifyArtist.Id})");
            }
            else
            {
                Console.WriteLine("⚠️ Not found on Spotify");
            }

            // 3. Create artist folder
            var artistFolderName = SanitizeFolderName(mbArtist.Name);
            var artistFolderPath = Path.Combine(LibraryPath, artistFolderName);

            if (Directory.Exists(artistFolderPath))
            {
                result.ErrorMessage = $"Artist folder already exists: {artistFolderName}";
                return result;
            }

            Directory.CreateDirectory(artistFolderPath);
            result.ArtistFolderPath = artistFolderPath;
            Console.WriteLine($"📁 Created folder: {artistFolderName}");

            // 4. Download photos from fanart.tv
            Console.WriteLine("🖼️ Downloading photos from fanart.tv...");
            var fanArtResult = await fanArtService.DownloadArtistPhotosAsync(
                mbArtist.Id,
                artistFolderPath
            );
            result.DownloadedPhotos = fanArtResult;

            // 5. Fetch LastFM monthly listeners (best-effort)
            long? monthlyListeners = null;
            List<JsonTopTrack> topTracks = new();
            try
            {
                var info = await lastfmClient.Artist.GetInfoByMbidAsync(mbArtist.Id);
                monthlyListeners = info?.Statistics?.Listeners;
                // Fetch top tracks as well
                var top = await lastfmClient.Artist.GetTopTracksByMbidAsync(mbArtist.Id);
                if (top != null)
                {
                    topTracks = top.Take(10)
                        .Select(t => new JsonTopTrack
                        {
                            Title = t.Name,
                            ReleaseTitle = t.Album?.Name,
                            CoverArt = null,
                            PlayCount = t.Statistics?.PlayCount,
                            TrackLength = t.Duration,
                        })
                        .ToList();
                }
            }
            catch { }

            // 6. Create artist.json
            var artistJson = new JsonArtist
            {
                Id = artistFolderName, // Use folder name as ID
                Name = mbArtist.Name,
                SortName = mbArtist.SortName ?? mbArtist.Name,
                MonthlyListeners = monthlyListeners,
                TopTracks = topTracks,
                Photos = new JsonArtistPhotos
                {
                    Thumbs = fanArtResult.Thumbs.Any() ? fanArtResult.Thumbs : null,
                    Backgrounds = fanArtResult.Backgrounds.Any() ? fanArtResult.Backgrounds : null,
                    Banners = fanArtResult.Banners.Any() ? fanArtResult.Banners : null,
                    Logos = fanArtResult.Logos.Any() ? fanArtResult.Logos : null,
                },
                Connections = new JsonArtistServiceConnections
                {
                    MusicBrainzArtistId = mbArtist.Id,
                    SpotifyId = spotifyArtist?.Id,
                },
            };

            // 7. Write artist.json file (centralized writer)
            var jsonWriter = new ServerLibrary.Writer.ServerLibraryJsonWriter();
            await jsonWriter.WriteArtistAsync(artistJson);

            Console.WriteLine($"✅ Created artist.json");

            // Attempt to map top tracks to local library entries by title
            try
            {
                if (artistJson.TopTracks != null && artistJson.TopTracks.Count > 0)
                {
                    // Read releases from disk for this artist
                    var releaseDirs = Directory.GetDirectories(artistFolderPath);
                    foreach (var releaseDir in releaseDirs)
                    {
                        var releaseJsonPath = Path.Combine(releaseDir, "release.json");
                        if (!File.Exists(releaseJsonPath))
                            continue;

                        var releaseJsonText = await File.ReadAllTextAsync(releaseJsonPath);
                        var releaseJson = JsonSerializer.Deserialize<JsonRelease>(
                            releaseJsonText,
                            GetJsonOptions()
                        );
                        if (releaseJson?.Tracks == null)
                            continue;

                        var folderName = Path.GetFileName(releaseDir) ?? string.Empty;
                        foreach (var top in artistJson.TopTracks)
                        {
                            if (top.ReleaseFolderName != null && top.TrackNumber != null)
                                continue; // already mapped

                            var match = releaseJson.Tracks.FirstOrDefault(t =>
                                string.Equals(
                                    t.Title,
                                    top.Title,
                                    StringComparison.OrdinalIgnoreCase
                                )
                            );
                            if (match != null)
                            {
                                top.ReleaseFolderName = folderName;
                                top.TrackNumber = match.TrackNumber;
                            }
                        }
                    }

                    // Rewrite artist.json if any mapping was added (centralized writer)
                    await jsonWriter.WriteArtistAsync(artistJson);
                }
            }
            catch { }

            // 8. Update cache
            await cache.UpdateCacheAsync();
            Console.WriteLine($"🔄 Updated cache");

            result.Success = true;
            result.ArtistJson = artistJson;
            Console.WriteLine($"🎉 Successfully imported artist: {artistName}");
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            Console.WriteLine($"❌ Error importing artist '{artistName}': {ex.Message}");

            // Cleanup on error
            if (
                !string.IsNullOrEmpty(result.ArtistFolderPath)
                && Directory.Exists(result.ArtistFolderPath)
            )
            {
                try
                {
                    Directory.Delete(result.ArtistFolderPath, true);
                    Console.WriteLine($"🧹 Cleaned up failed import folder");
                }
                catch (Exception cleanupEx)
                {
                    Console.WriteLine($"⚠️ Failed to cleanup folder: {cleanupEx.Message}");
                }
            }
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

        try
        {
            Console.WriteLine($"💿 Importing releases for artist: {artistId}");

            // 1. Get artist info to find MusicBrainz ID
            var artist = await cache.GetArtistByIdAsync(artistId);
            if (artist?.JsonArtist.Connections?.MusicBrainzArtistId == null)
            {
                result.ErrorMessage = "Artist not found or missing MusicBrainz ID";
                return result;
            }

            var mbArtistId = artist.JsonArtist.Connections.MusicBrainzArtistId;
            var artistFolderPath = Path.Combine(LibraryPath, artistId);

            // 2. Get release groups from MusicBrainz
            Console.WriteLine("🔍 Fetching release groups from MusicBrainz...");
            var releaseGroups = await musicBrainzService.GetArtistReleaseGroupsAsync(mbArtistId);

            Console.WriteLine($"📀 Found {releaseGroups.Count} release groups");

            // 3. Import each release group
            foreach (var releaseGroup in releaseGroups)
            {
                try
                {
                    var releaseResult = await ImportReleaseGroupAsync(
                        releaseGroup,
                        artistFolderPath,
                        artistId
                    );
                    result.ImportedReleases.Add(releaseResult);

                    if (releaseResult.Success)
                    {
                        Console.WriteLine($"✅ Imported release: {releaseGroup.Title}");
                    }
                    else
                    {
                        Console.WriteLine(
                            $"❌ Failed to import release: {releaseGroup.Title} - {releaseResult.ErrorMessage}"
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"❌ Error importing release '{releaseGroup.Title}': {ex.Message}"
                    );
                    result.ImportedReleases.Add(
                        new SingleReleaseImportResult
                        {
                            ReleaseGroupId = releaseGroup.Id,
                            Title = releaseGroup.Title,
                            ErrorMessage = ex.Message,
                        }
                    );
                }
            }

            // 4. Update cache
            await cache.UpdateCacheAsync();
            Console.WriteLine($"🔄 Updated cache");

            result.Success = result.ImportedReleases.Any(r => r.Success);
            Console.WriteLine(
                $"🎉 Import complete: {result.ImportedReleases.Count(r => r.Success)}/{result.ImportedReleases.Count} releases imported"
            );
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            Console.WriteLine($"❌ Error importing releases for artist '{artistId}': {ex.Message}");
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
        return await releaseImporter.ImportReleaseGroupAsync(releaseGroup, artistFolderPath, artistId);
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
