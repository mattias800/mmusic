using System.Text.Json;
using System.Text.Json.Serialization;
using Hqub.Lastfm;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ServerLibrary.Utils;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Features.ServerSettings;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import.Services;

/// <summary>
/// Builds a JsonRelease from MusicBrainz data and optional local context (audio files, artist.json).
/// Centralizes all mapping/enrichment to be the single source of truth.
/// </summary>
public class ReleaseJsonBuilder(
    MusicBrainzService musicBrainzService,
    CoverArtDownloadService coverArtDownloadService,
    LastfmClient lastfmClient,
    ServerSettingsAccessor serverSettingsAccessor,
    ILogger<ReleaseJsonBuilder> logger
)
{
    private static readonly string[] AudioExtensions = [".mp3", ".flac", ".wav", ".m4a", ".ogg"];

    public async Task<JsonRelease?> BuildAsync(
        string artistDir,
        string releaseGroupId,
        string releaseFolderName,
        string? releaseTitle,
        string? primaryType
    )
    {
        var startTime = DateTime.UtcNow;
        logger.LogInformation("[ReleaseBuilder] 🚀 Starting to build release.json for '{Title}' (Type: {PrimaryType}, RG ID: {ReleaseGroupId})", 
            releaseTitle, primaryType, releaseGroupId);

        try
        {
            // 1. Fetch candidate releases for the RG
            logger.LogInformation("[ReleaseBuilder] 🔍 Step 1: Fetching candidate releases from MusicBrainz for release group: {ReleaseGroupId}", releaseGroupId);
            var releasesStart = DateTime.UtcNow;
            var releases = await musicBrainzService.GetReleasesForReleaseGroupAsync(releaseGroupId);
            var releasesDuration = DateTime.UtcNow - releasesStart;
            
            logger.LogInformation("[ReleaseBuilder] 📀 Found {ReleaseCount} candidate releases in {DurationMs}ms", releases.Count, releasesDuration.TotalMilliseconds);
            
            // Log detailed MusicBrainz response for debugging
            foreach (var release in releases.Take(3)) // Log first 3 releases to avoid spam
            {
                logger.LogInformation("[ReleaseBuilder] 📋 Release '{Title}' (ID: {ReleaseId}):", release.Title, release.Id);
                logger.LogInformation("[ReleaseBuilder]   - Date: {Date}, Country: {Country}, Status: {Status}", release.Date, release.Country, release.Status);
                
                if (release.ReleaseGroup?.Credits?.Any() == true)
                {
                    logger.LogInformation("[ReleaseBuilder]   - Release Group Credits ({CreditCount}):", release.ReleaseGroup.Credits.Count());
                    foreach (var credit in release.ReleaseGroup.Credits.Take(5)) // Limit to first 5 credits
                    {
                        logger.LogInformation("[ReleaseBuilder]     * {ArtistName} (ID: {ArtistId}, Join: '{JoinPhrase}')", 
                            credit.Name ?? credit.Artist?.Name ?? "Unknown", 
                            credit.Artist?.Id ?? "Unknown", 
                            credit.JoinPhrase ?? "");
                    }
                }
                else
                {
                    logger.LogInformation("[ReleaseBuilder]   - No release group credits found");
                }
                
                if (release.Media?.Any() == true)
                {
                    var totalTracks = release.Media.Sum(m => m.Tracks?.Count ?? 0);
                    logger.LogInformation("[ReleaseBuilder]   - Media: {MediaCount} media with {TotalTracks} total tracks", release.Media.Count(), totalTracks);
                }
            }

            // Filter out demo release groups entirely
            var beforeFilter = releases.Count;
            releases = releases
                .Where(r => r.ReleaseGroup != null && !r.ReleaseGroup.IsDemo())
                .ToList();
            var afterFilter = releases.Count;
            
            if (beforeFilter != afterFilter)
            {
                logger.LogInformation("[ReleaseBuilder] 🚫 Filtered out {FilteredCount} demo releases, {RemainingCount} remaining", 
                    beforeFilter - afterFilter, afterFilter);
            }

            // Do not persist possible track counts in JSON. We'll compute on-demand during downloads.

            // 2. Evaluate local audio files (if any) to influence selection
            logger.LogInformation("[ReleaseBuilder] 🎵 Step 2: Evaluating local audio files in release directory");
            var releaseDir = Path.Combine(artistDir, releaseFolderName);
            List<string> audioFiles = [];
            int audioFileCount = 0;
            if (Directory.Exists(releaseDir))
            {
                audioFiles = Directory
                    .GetFiles(releaseDir)
                    .Where(f => AudioExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                audioFileCount = audioFiles.Count;
                logger.LogInformation("[ReleaseBuilder] ✅ Found {AudioFileCount} audio files in release directory: {ReleaseDir}", audioFileCount, releaseDir);
            }
            else
            {
                logger.LogInformation("[ReleaseBuilder] ℹ️ Release directory does not exist yet: {ReleaseDir}", releaseDir);
            }

            // 3. Check for existing override
            logger.LogInformation("[ReleaseBuilder] 🔧 Step 3: Checking for existing release override");
            Hqub.MusicBrainz.Entities.Release? selected = null;
            string? existingOverrideId = null;
            try
            {
                var existingPath = Path.Combine(releaseDir, "release.json");
                if (File.Exists(existingPath))
                {
                    logger.LogInformation("[ReleaseBuilder] 📄 Found existing release.json, checking for override");
                    var txt = await File.ReadAllTextAsync(existingPath);
                    var existing = JsonSerializer.Deserialize<JsonRelease>(txt, GetJsonOptions());
                    var overrideId = existing?.Connections?.MusicBrainzReleaseIdOverride;
                    if (!string.IsNullOrWhiteSpace(overrideId))
                    {
                        existingOverrideId = overrideId;
                        logger.LogInformation("[ReleaseBuilder] 🎯 Found existing override to release ID: {OverrideId}", overrideId);
                        var exact = await musicBrainzService.GetReleaseByIdAsync(overrideId!);
                        if (exact != null)
                        {
                            selected = exact;
                            logger.LogInformation("[ReleaseBuilder] ✅ Successfully resolved override to release: '{Title}'", exact.Title);
                        }
                        else
                        {
                            logger.LogWarning("[ReleaseBuilder] ⚠️ Failed to resolve override release ID: {OverrideId}", overrideId);
                        }
                    }
                    else
                    {
                        logger.LogInformation("[ReleaseBuilder] ℹ️ No override found in existing release.json");
                    }
                }
                else
                {
                    logger.LogInformation("[ReleaseBuilder] ℹ️ No existing release.json found");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[ReleaseBuilder] ⚠️ Error checking for existing override");
            }

            // 4. Select the best fitting release
            if (selected == null)
            {
                logger.LogInformation("[ReleaseBuilder] 🎯 Step 4: Selecting best release from {CandidateCount} candidates", releases.Count);
                var selectionStart = DateTime.UtcNow;
                
                // Selection logic here - you can add more detailed logging for the selection process
                if (releases.Count > 0)
                {
                    selected = releases.First(); // Simplified selection for now
                    logger.LogInformation("[ReleaseBuilder] ✅ Selected release: '{Title}' (ID: {ReleaseId}) from {CandidateCount} candidates", 
                        selected.Title, selected.Id, releases.Count);
                }
                else
                {
                    logger.LogWarning("[ReleaseBuilder] ⚠️ No suitable releases found for release group");
                    return null;
                }
                
                var selectionDuration = DateTime.UtcNow - selectionStart;
                logger.LogInformation("[ReleaseBuilder] ⏱️ Release selection completed in {DurationMs}ms", selectionDuration.TotalMilliseconds);
            }

            // 5. Download cover art
            logger.LogInformation("[ReleaseBuilder] 🖼️ Step 5: Downloading cover art for release '{Title}'", selected.Title);
            var coverArtStart = DateTime.UtcNow;
            var coverArtRelPath = await coverArtDownloadService.DownloadReleaseCoverArtAsync(
                releaseGroupId,
                releaseDir
            );
            var coverArtDuration = DateTime.UtcNow - coverArtStart;
            
            if (!string.IsNullOrWhiteSpace(coverArtRelPath))
            {
                logger.LogInformation("[ReleaseBuilder] ✅ Cover art downloaded successfully in {DurationMs}ms: {CoverArtPath}", 
                    coverArtDuration.TotalMilliseconds, coverArtRelPath);
            }
            else
            {
                logger.LogInformation("[ReleaseBuilder] ℹ️ No cover art available for this release");
            }

            // 6. Map release type
            logger.LogInformation("[ReleaseBuilder] 🏷️ Step 6: Mapping release type and metadata");
            logger.LogInformation("[ReleaseBuilder] 🔍 Raw primaryType from MusicBrainz: '{PrimaryType}'", primaryType);
            
            var releaseType = primaryType?.ToLowerInvariant() switch
            {
                "album" => JsonReleaseType.Album,
                "ep" => JsonReleaseType.Ep,
                "single" => JsonReleaseType.Single,
                "compilation" => JsonReleaseType.Album, // Treat compilations as albums
                "soundtrack" => JsonReleaseType.Album,  // Treat soundtracks as albums
                "live" => JsonReleaseType.Album,        // Treat live releases as albums
                "remix" => JsonReleaseType.Ep,          // Treat remix releases as EPs
                "mixtape" => JsonReleaseType.Ep,        // Treat mixtapes as EPs
                _ => JsonReleaseType.Album, // Default fallback
            };
            
            logger.LogInformation("[ReleaseBuilder] ✅ Mapped release type: '{PrimaryType}' → {ReleaseType}", primaryType, releaseType);
            
            // Additional logging for debugging
            if (primaryType != null && !new[] { "album", "ep", "single", "compilation", "soundtrack", "live", "remix", "mixtape" }.Contains(primaryType.ToLowerInvariant()))
            {
                logger.LogWarning("[ReleaseBuilder] ⚠️ Unknown primary type '{PrimaryType}' - defaulting to Album", primaryType);
            }

            // 7. Build artist ID mapping
            logger.LogInformation("[ReleaseBuilder] 👥 Step 7: Building MusicBrainz to local artist ID mapping");
            var mappingStart = DateTime.UtcNow;
            var mbToLocalArtistId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var libraryRoot = (await serverSettingsAccessor.GetAsync()).LibraryPath;
                if (Directory.Exists(libraryRoot))
                {
                    var artistDirs = Directory.GetDirectories(libraryRoot);
                    logger.LogInformation("[ReleaseBuilder] 🔍 Scanning {ArtistDirCount} artist directories for MBID mappings", artistDirs.Length);
                    
                    foreach (var artistPath in artistDirs)
                    {
                        try
                        {
                            var artistJsonPath = Path.Combine(artistPath, "artist.json");
                            if (!File.Exists(artistJsonPath))
                                continue;
                            var text = await File.ReadAllTextAsync(artistJsonPath);
                            var jsonArtist = JsonSerializer.Deserialize<JsonArtist>(
                                text,
                                GetJsonOptions()
                            );
                            var mbId = jsonArtist?.Connections?.MusicBrainzArtistId;
                            if (
                                !string.IsNullOrWhiteSpace(mbId)
                                && !string.IsNullOrWhiteSpace(jsonArtist?.Id)
                            )
                            {
                                mbToLocalArtistId[mbId!] = jsonArtist!.Id!;
                            }
                        }
                        catch { }
                    }
                    
                    logger.LogInformation("[ReleaseBuilder] ✅ Built {MappingCount} MBID to local artist ID mappings", mbToLocalArtistId.Count);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[ReleaseBuilder] ⚠️ Error building artist ID mapping");
            }
            var mappingDuration = DateTime.UtcNow - mappingStart;
            logger.LogInformation("[ReleaseBuilder] ⏱️ Artist ID mapping completed in {DurationMs}ms", mappingDuration.TotalMilliseconds);

            // 8. Fetch recordings with credits
            logger.LogInformation("[ReleaseBuilder] 🎵 Step 8: Fetching recordings and track data for release '{Title}'", selected.Title);
            var recordingsStart = DateTime.UtcNow;
            var recordings = await musicBrainzService.GetRecordingsForReleaseAsync(selected.Id);
            var recordingsDuration = DateTime.UtcNow - recordingsStart;
            
            logger.LogInformation("[ReleaseBuilder] ✅ Fetched {RecordingCount} recordings in {DurationMs}ms", recordings.Count, recordingsDuration.TotalMilliseconds);
            
            var recById = recordings.ToDictionary(r => r.Id, r => r);

            // 9. Build tracks
            logger.LogInformation("[ReleaseBuilder] 🎼 Step 9: Building track list from {RecordingCount} recordings", recordings.Count);
            var tracksStart = DateTime.UtcNow;
            
            var tracks = selected
                .Media
                .SelectMany(m => m.Tracks ?? new List<Hqub.MusicBrainz.Entities.Track>())
                .Where(t => t != null)
                .Select(t =>
                {
                    var recordingId = t.Recording?.Id;
                    var trackCredits = new List<JsonTrackCredit>();

                    if (recById.TryGetValue(recordingId!, out var recording))
                    {
                        // Add artist credits
                        if (recording.Credits != null)
                        {
                            foreach (var ac in recording.Credits)
                            {
                                if (mbToLocalArtistId.TryGetValue(ac.Artist.Id, out var localId))
                                {
                                    trackCredits.Add(new JsonTrackCredit
                                    {
                                        ArtistName = ac.Name ?? ac.Artist.Name ?? string.Empty,
                                        ArtistId = localId,
                                        MusicBrainzArtistId = ac.Artist.Id,
                                    });
                                }
                            }
                        }
                    }

                    return new JsonTrack
                    {
                        Title = t.Recording?.Title ?? string.Empty,
                        TrackNumber = t.Position,
                        TrackLength = t.Length,
                        Connections = string.IsNullOrWhiteSpace(recordingId)
                            ? null
                            : new JsonTrackServiceConnections { MusicBrainzRecordingId = recordingId },
                        Credits = trackCredits,
                    };
                })
                .Where(t => t.TrackNumber > 0)
                .OrderBy(t => t.TrackNumber)
                .ToList();

            var tracksDuration = DateTime.UtcNow - tracksStart;
            logger.LogInformation("[ReleaseBuilder] ✅ Built {TrackCount} tracks in {DurationMs}ms", tracks.Count, tracksDuration.TotalMilliseconds);

            // 10. Enrich with Last.fm statistics
            logger.LogInformation("[ReleaseBuilder] 📊 Step 10: Enriching tracks with Last.fm statistics");
            var lastfmStart = DateTime.UtcNow;
            try
            {
                string? artistDisplayName = null;
                var artistJsonPath = Path.Combine(artistDir, "artist.json");
                if (File.Exists(artistJsonPath))
                {
                    var text = await File.ReadAllTextAsync(artistJsonPath);
                    var jsonArtist = JsonSerializer.Deserialize<JsonArtist>(text, GetJsonOptions());
                    artistDisplayName = jsonArtist?.Name;
                }

                if (!string.IsNullOrWhiteSpace(artistDisplayName) && tracks != null)
                {
                    var enrichedTracks = 0;
                    foreach (var jt in tracks)
                    {
                        try
                        {
                            var info = await lastfmClient.Track.GetInfoAsync(
                                jt.Title,
                                artistDisplayName
                            );
                            if (info?.Statistics != null)
                            {
                                jt.Statistics = new JsonTrackStatistics
                                {
                                    PlayCount = info.Statistics.PlayCount,
                                    Listeners = info.Statistics.Listeners,
                                };
                                jt.PlayCount = jt.Statistics.PlayCount;
                                enrichedTracks++;
                            }
                        }
                        catch { }
                    }
                    logger.LogInformation("[ReleaseBuilder] ✅ Enriched {EnrichedCount}/{TotalTracks} tracks with Last.fm data", enrichedTracks, tracks.Count);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[ReleaseBuilder] ⚠️ Error enriching tracks with Last.fm data");
            }
            var lastfmDuration = DateTime.UtcNow - lastfmStart;
            logger.LogInformation("[ReleaseBuilder] ⏱️ Last.fm enrichment completed in {DurationMs}ms", lastfmDuration.TotalMilliseconds);

            // 11. Map audio file paths
            logger.LogInformation("[ReleaseBuilder] 🎵 Step 11: Mapping audio file paths to tracks");
            if (tracks != null && tracks.Count > 0 && audioFiles.Count > 0)
            {
                var fileNames = audioFiles.Select(Path.GetFileName).ToList();
                var mappedTracks = 0;
                foreach (var track in tracks)
                {
                    var index = track.TrackNumber - 1;
                    if (index >= 0 && index < fileNames.Count)
                    {
                        track.AudioFilePath = "./" + fileNames[index];
                        mappedTracks++;
                    }
                }
                logger.LogInformation("[ReleaseBuilder] ✅ Mapped audio files to {MappedCount}/{TotalTracks} tracks", mappedTracks, tracks.Count);
            }
            else
            {
                logger.LogInformation("[ReleaseBuilder] ℹ️ No audio files to map to tracks");
            }

            // 12. Build final JsonRelease
            logger.LogInformation("[ReleaseBuilder] 🏗️ Step 12: Building final JsonRelease object");
            
            // Read artist information from artist.json
            string localArtistName = string.Empty;
            string? localArtistId = null;
            try
            {
                var artistJsonPath = Path.Combine(artistDir, "artist.json");
                if (File.Exists(artistJsonPath))
                {
                    var text = await File.ReadAllTextAsync(artistJsonPath);
                    var jsonArtist = JsonSerializer.Deserialize<JsonArtist>(text, GetJsonOptions());
                    localArtistName = jsonArtist?.Name ?? string.Empty;
                    localArtistId = jsonArtist?.Id;
                    logger.LogInformation("[ReleaseBuilder] ✅ Read artist info from artist.json: Name='{ArtistName}', ID='{ArtistId}'", localArtistName, localArtistId);
                }
                else
                {
                    logger.LogWarning("[ReleaseBuilder] ⚠️ artist.json not found at {ArtistJsonPath}", artistJsonPath);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[ReleaseBuilder] ⚠️ Error reading artist info from artist.json");
            }
            
            // Get artist name from MusicBrainz (preserves historical accuracy)
            string musicBrainzArtistName = string.Empty;
            string? musicBrainzArtistId = null;
            if (selected?.ReleaseGroup?.Credits?.Any() == true)
            {
                var primaryCredit = selected.ReleaseGroup.Credits.FirstOrDefault();
                if (primaryCredit != null)
                {
                    musicBrainzArtistName = primaryCredit.Name ?? primaryCredit.Artist?.Name ?? "Unknown";
                    musicBrainzArtistId = primaryCredit.Artist?.Id;
                    logger.LogInformation("[ReleaseBuilder] 🔍 MusicBrainz primary artist credit: '{MbArtistName}' (ID: {MbArtistId})", 
                        musicBrainzArtistName, musicBrainzArtistId);
                    
                    if (!string.IsNullOrEmpty(localArtistName) && !string.IsNullOrEmpty(musicBrainzArtistName) && 
                        !string.Equals(localArtistName, musicBrainzArtistName, StringComparison.OrdinalIgnoreCase))
                    {
                        logger.LogInformation("[ReleaseBuilder] ℹ️ Artist name change detected: Local '{LocalName}' vs MusicBrainz '{MbName}' (historical)", 
                            localArtistName, musicBrainzArtistName);
                    }
                }
            }
            else
            {
                logger.LogWarning("[ReleaseBuilder] ⚠️ No MusicBrainz artist credits found for selected release");
            }
            
            // Use MusicBrainz artist name for historical accuracy, but fall back to local if needed
            var finalArtistName = !string.IsNullOrEmpty(musicBrainzArtistName) ? musicBrainzArtistName : localArtistName;
            logger.LogInformation("[ReleaseBuilder] ✅ Final artist name for release: '{FinalArtistName}' (from MusicBrainz: {FromMb}, Local ID: {LocalId})", 
                finalArtistName, !string.IsNullOrEmpty(musicBrainzArtistName), localArtistId);
            
            // Extract label information from MusicBrainz release
            var labels = new List<JsonLabelInfo>();
            if (selected?.Labels != null && selected.Labels.Any())
            {
                logger.LogInformation("[ReleaseBuilder] 🏷️ Found {LabelCount} labels for release '{Title}'", selected.Labels.Count(), selected.Title);
                foreach (var labelInfo in selected.Labels)
                {
                    var jsonLabel = new JsonLabelInfo
                    {
                        Name = labelInfo.Label?.Name ?? string.Empty,
                        Id = labelInfo.Label?.Id,
                        CatalogNumber = labelInfo.CatalogNumber,
                        Disambiguation = labelInfo.Label?.Disambiguation
                    };
                    labels.Add(jsonLabel);
                    
                    logger.LogInformation("[ReleaseBuilder] 🏷️ Label: '{LabelName}' (ID: {LabelId}, Catalog: {CatalogNumber})", 
                        jsonLabel.Name, jsonLabel.Id, jsonLabel.CatalogNumber ?? "N/A");
                }
            }
            else
            {
                logger.LogInformation("[ReleaseBuilder] ℹ️ No label information found for release '{Title}'", selected?.Title ?? "Unknown");
            }
            
            var finalRelease = new JsonRelease
            {
                Title = releaseTitle ?? releaseFolderName,
                SortTitle = releaseTitle,
                ArtistName = finalArtistName,
                ArtistId = localArtistId,
                Type = releaseType,
                FirstReleaseDate = selected?.ReleaseGroup?.FirstReleaseDate,
                FirstReleaseYear =
                    selected?.ReleaseGroup?.FirstReleaseDate?.Length >= 4
                        ? selected!.ReleaseGroup!.FirstReleaseDate!.Substring(0, 4)
                        : null,
                CoverArt = coverArtRelPath,
                Labels = labels.Count > 0 ? labels : null,
                Tracks = tracks != null && tracks.Count > 0 ? tracks : null,
                Connections = new ReleaseServiceConnections
                {
                    MusicBrainzReleaseGroupId = releaseGroupId,
                    MusicBrainzSelectedReleaseId = selected?.Id,
                    // If an explicit override was used, keep it; otherwise leave null
                    MusicBrainzReleaseIdOverride = existingOverrideId,
                },
            };

            var totalDuration = DateTime.UtcNow - startTime;
            logger.LogInformation("[ReleaseBuilder] 🎉 Successfully built release.json for '{Title}' in {TotalDurationMs}ms", releaseTitle, totalDuration.TotalMilliseconds);
            logger.LogInformation("[ReleaseBuilder] 📊 Build Summary: Releases fetch: {ReleasesMs}ms, Cover art: {CoverArtMs}ms, Recordings: {RecordingsMs}ms, Tracks: {TracksMs}ms, Last.fm: {LastfmMs}ms", 
                releasesDuration.TotalMilliseconds, coverArtDuration.TotalMilliseconds, recordingsDuration.TotalMilliseconds, 
                tracksDuration.TotalMilliseconds, lastfmDuration.TotalMilliseconds);

            return finalRelease;
        }
        catch (Exception ex)
        {
            var totalDuration = DateTime.UtcNow - startTime;
            logger.LogError(ex, "[ReleaseBuilder] ❌ Failed to build release.json for '{Title}' after {TotalDurationMs}ms", releaseTitle, totalDuration.TotalMilliseconds);
            throw;
        }
    }

    private static string Normalize(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return string.Empty;
        var cleaned = new string(
            s.ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : ' ').ToArray()
        );
        // collapse spaces
        var parts = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", parts);
    }

    private static JsonSerializerOptions GetJsonOptions() =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };
}
