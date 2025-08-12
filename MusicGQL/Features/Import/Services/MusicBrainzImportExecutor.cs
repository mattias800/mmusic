using System.Text.Json;
using System.Text.Json.Serialization;
using Hqub.Lastfm;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ServerLibrary.Utils;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Features.ArtistImportQueue;
using MusicGQL.Features.ArtistImportQueue.Services;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import.Services;

public interface IImportExecutor
{
    Task ImportOrEnrichArtistAsync(string artistDir, string mbArtistId, string artistDisplayName);

    Task ImportReleaseIfMissingAsync(
        string artistDir,
        string releaseDir,
        string releaseGroupId,
        string? releaseTitle,
        string? primaryType
    );

    Task<int> ImportEligibleReleaseGroupsAsync(string artistDir, string mbArtistId);
}

public sealed class MusicBrainzImportExecutor(
    MusicBrainzService musicBrainzService,
    CoverArtDownloadService coverArtDownloadService,
    LastfmClient lastfmClient,
    Integration.Spotify.SpotifyService spotifyService,
    ILogger<MusicBrainzImportExecutor> logger,
    ReleaseJsonBuilder releaseJsonBuilder,
    ServerLibrary.Writer.ServerLibraryJsonWriter writer,
    CurrentArtistImportStateService progressService
) : IImportExecutor
{
    private static readonly string[] AudioExtensions = [".mp3", ".flac", ".wav", ".m4a", ".ogg"];

    private static string NormalizeTitle(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        var s = input.Replace("’", "'").Replace("“", "\"").Replace("”", "\"");
        var builder = new System.Text.StringBuilder(s.Length);
        foreach (var ch in s)
        {
            if (char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
        }

        var normalized = System
            .Text.RegularExpressions.Regex.Replace(builder.ToString(), "\\s+", " ")
            .Trim();
        return normalized;
    }

    private static string StripParentheses(string input)
    {
        return System
            .Text.RegularExpressions.Regex.Replace(input, "\\(.*?\\)", string.Empty)
            .Trim();
    }

    private static bool AreTitlesEquivalent(string a, string b)
    {
        var na = NormalizeTitle(a);
        var nb = NormalizeTitle(b);
        if (na.Equals(nb, StringComparison.Ordinal))
            return true;
        var npa = NormalizeTitle(StripParentheses(a));
        var npb = NormalizeTitle(StripParentheses(b));
        return npa.Equals(npb, StringComparison.Ordinal);
    }

    private static string SanitizeFolderName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join(
            "",
            name.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)
        );
        return sanitized.Trim();
    }

    public async Task ImportOrEnrichArtistAsync(
        string artistDir,
        string mbArtistId,
        string artistDisplayName
    )
    {
        var artistJsonPath = Path.Combine(artistDir, "artist.json");
        JsonArtist? jsonArtist = null;
        bool created = false;

        if (!File.Exists(artistJsonPath))
        {
            logger.LogInformation("[ImportArtist] Creating new artist.json for '{Artist}' (MBID {MbId}) at {Path}", artistDisplayName, mbArtistId, artistJsonPath);
            var photos = await coverArtDownloadService.DownloadArtistPhotosAsync(
                mbArtistId,
                artistDir
            );
            jsonArtist = new JsonArtist
            {
                Id = Path.GetFileName(artistDir) ?? artistDisplayName,
                Name = artistDisplayName,
                Photos = new JsonArtistPhotos
                {
                    Thumbs = photos.Thumbs.Any() ? photos.Thumbs : null,
                    Backgrounds = photos.Backgrounds.Any() ? photos.Backgrounds : null,
                    Banners = photos.Banners.Any() ? photos.Banners : null,
                    Logos = photos.Logos.Any() ? photos.Logos : null,
                },
                Connections = new JsonArtistServiceConnections { MusicBrainzArtistId = mbArtistId },
            };
            created = true;
        }
        else
        {
            logger.LogInformation("[ImportArtist] Enriching existing artist.json for '{Artist}' (MBID {MbId}) at {Path}", artistDisplayName, mbArtistId, artistJsonPath);
            try
            {
                var text = await File.ReadAllTextAsync(artistJsonPath);
                jsonArtist =
                    JsonSerializer.Deserialize<JsonArtist>(text, GetJsonOptions())
                    ?? new JsonArtist();
            }
            catch
            {
                jsonArtist = new JsonArtist
                {
                    Id = Path.GetFileName(artistDir) ?? artistDisplayName,
                    Name = artistDisplayName,
                };
            }

            // ensure connections
            jsonArtist.Connections ??= new JsonArtistServiceConnections();
            if (string.IsNullOrWhiteSpace(jsonArtist.Connections.MusicBrainzArtistId))
            {
                jsonArtist.Connections.MusicBrainzArtistId = mbArtistId;
            }

            if (string.IsNullOrWhiteSpace(jsonArtist.Name))
                jsonArtist.Name = artistDisplayName;
            if (string.IsNullOrWhiteSpace(jsonArtist.Id))
                jsonArtist.Id = Path.GetFileName(artistDir) ?? artistDisplayName;
            // Always refresh artist photos during enrich to keep assets up to date
            try
            {
                var photos = await coverArtDownloadService.DownloadArtistPhotosAsync(
                    mbArtistId,
                    artistDir
                );
                jsonArtist.Photos = new JsonArtistPhotos
                {
                    Thumbs = photos.Thumbs.Any() ? photos.Thumbs : null,
                    Backgrounds = photos.Backgrounds.Any() ? photos.Backgrounds : null,
                    Banners = photos.Banners.Any() ? photos.Banners : null,
                    Logos = photos.Logos.Any() ? photos.Logos : null,
                };
            }
            catch { }
        }

        // Fetch Last.fm enrichment (only if missing or we just created)
        try
        {
            if (
                created
                || jsonArtist.MonthlyListeners == null
                || jsonArtist.TopTracks == null
                || jsonArtist.TopTracks.Count == 0
            )
            {
                var info = await lastfmClient.Artist.GetInfoByMbidAsync(mbArtistId);
                jsonArtist.MonthlyListeners =
                    info?.Statistics?.Listeners ?? jsonArtist.MonthlyListeners;

                // TOP TRACKS VIA IMPORTER (switchable)
                // For now, hardcode Last.fm importer; switch to Spotify importer by replacing this line
                TopTracks.ITopTracksImporter importer = new TopTracks.TopTracksLastFmImporter(
                    lastfmClient
                );
                jsonArtist.TopTracks = await importer.GetTopTracksAsync(mbArtistId, 10);

                // Attempt to map stored top tracks to local library tracks to enable playback
                try
                {
                    if (jsonArtist.TopTracks != null && jsonArtist.TopTracks.Count > 0)
                    {
                        var releaseDirs = Directory.GetDirectories(artistDir);
                        foreach (var releaseDir in releaseDirs)
                        {
                            var releaseJsonPath = Path.Combine(releaseDir, "release.json");
                            if (!File.Exists(releaseJsonPath))
                                continue;

                            JsonRelease? releaseJson = null;
                            try
                            {
                                var releaseText = await File.ReadAllTextAsync(releaseJsonPath);
                                releaseJson = JsonSerializer.Deserialize<JsonRelease>(
                                    releaseText,
                                    GetJsonOptions()
                                );
                            }
                            catch
                            {
                                continue;
                            }

                            if (releaseJson?.Tracks == null)
                                continue;

                            var folderName = Path.GetFileName(releaseDir) ?? string.Empty;
                            foreach (var topTrack in jsonArtist.TopTracks)
                            {
                                if (
                                    topTrack.ReleaseFolderName != null
                                    && topTrack.TrackNumber != null
                                )
                                    continue;

                                var match = releaseJson.Tracks.FirstOrDefault(t =>
                                    !string.IsNullOrWhiteSpace(t.Title)
                                    && AreTitlesEquivalent(t.Title, topTrack.Title)
                                );

                                if (match != null)
                                {
                                    topTrack.ReleaseFolderName = folderName;
                                    topTrack.TrackNumber = match.TrackNumber;
                                    topTrack.ReleaseTitle = releaseJson.Title;
                                    if (!string.IsNullOrWhiteSpace(releaseJson.CoverArt))
                                    {
                                        var relPath = releaseJson.CoverArt.StartsWith("./")
                                            ? releaseJson.CoverArt[2..]
                                            : releaseJson.CoverArt;
                                        // Store path relative to artist folder so it resolves correctly from artist.json
                                        var combined = Path.Combine(folderName, relPath)
                                            .Replace('\\', '/');
                                        topTrack.CoverArt = "./" + combined;
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // ignore mapping failures
                }

                // Complete missing fields (releaseTitle/cover art) using Spotify fallback
                try
                {
                    var completer = new TopTracks.TopTracksCompleter(spotifyService);
                    await completer.CompleteAsync(artistDir, jsonArtist);
                }
                catch { }
            }
        }
        catch
        {
            // ignore Last.fm failures
        }

        // Try to enrich external service connections using MusicBrainz relations and Spotify fallback
        try
        {
            jsonArtist.Connections ??= new JsonArtistServiceConnections();
            var mbArtist = await musicBrainzService.GetArtistByIdAsync(mbArtistId);
            var rels = mbArtist?.Relations;
            if (rels != null)
            {
                foreach (var rel in rels)
                {
                    var url = rel?.Url?.Resource;
                    if (string.IsNullOrWhiteSpace(url)) continue;
                    if (url.Contains("open.spotify.com/artist/", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = new Uri(url).Segments;
                        var id = parts.LastOrDefault()?.Trim('/');
                        if (!string.IsNullOrWhiteSpace(id)) jsonArtist.Connections.SpotifyId ??= id;
                    }
                    else if (url.Contains("music.apple.com", StringComparison.OrdinalIgnoreCase))
                    {
                        // Apple Music artist URLs often include /artist/<name>/<id>
                        var segments = new Uri(url).Segments.Select(s => s.Trim('/')).ToArray();
                        var maybeId = segments.LastOrDefault();
                        if (!string.IsNullOrWhiteSpace(maybeId) && maybeId.All(char.IsDigit))
                            jsonArtist.Connections.AppleMusicArtistId ??= maybeId;
                    }
                    else if (url.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) || url.Contains("youtu.be", StringComparison.OrdinalIgnoreCase))
                    {
                        // Prefer channel URLs
                        if (url.Contains("/channel/") || url.Contains("/c/") || url.Contains("/@"))
                            jsonArtist.Connections.YoutubeChannelUrl ??= url;
                    }
                    else if (url.Contains("tidal.com", StringComparison.OrdinalIgnoreCase))
                    {
                        // Not always standardized; keep as id when possible
                        jsonArtist.Connections.TidalArtistId ??= url;
                    }
                    else if (url.Contains("deezer.com", StringComparison.OrdinalIgnoreCase))
                    {
                        jsonArtist.Connections.DeezerArtistId ??= url.Split('/').LastOrDefault();
                    }
                    else if (url.Contains("soundcloud.com", StringComparison.OrdinalIgnoreCase))
                    {
                        jsonArtist.Connections.SoundcloudUrl ??= url;
                    }
                    else if (url.Contains("bandcamp.com", StringComparison.OrdinalIgnoreCase))
                    {
                        jsonArtist.Connections.BandcampUrl ??= url;
                    }
                    else if (url.Contains("discogs.com", StringComparison.OrdinalIgnoreCase))
                    {
                        jsonArtist.Connections.DiscogsUrl ??= url;
                    }
                }
            }

            // If SpotifyId still missing, try to search by name as a fallback
            if (string.IsNullOrWhiteSpace(jsonArtist.Connections.SpotifyId))
            {
                try
                {
                    var spotifyMatches = await spotifyService.SearchArtistsAsync(artistDisplayName, 1);
                    var match = spotifyMatches?.FirstOrDefault();
                    if (match?.Id != null)
                    {
                        jsonArtist.Connections.SpotifyId = match.Id;
                    }
                }
                catch { }
            }
        }
        catch
        {
            // ignore enrich errors
        }

        var artistJson = JsonSerializer.Serialize(jsonArtist, GetJsonOptions());
        await File.WriteAllTextAsync(artistJsonPath, artistJson);
        logger.LogInformation("[ImportArtist] Wrote artist.json for '{Artist}' at {Path}", jsonArtist.Name, artistJsonPath);
    }

    public async Task<int> ImportEligibleReleaseGroupsAsync(string artistDir, string mbArtistId)
    {
        var releaseGroups = await musicBrainzService.GetReleaseGroupsForArtistAsync(mbArtistId);
        var eligible = releaseGroups.Where(rg => LibraryDecider.ShouldBeAddedWhenAddingArtistToServerLibrary(rg)).ToList();
        int totalEligible = eligible.Count;
        int created = 0;
        int processed = 0;

        // Update state to indicate we are now importing releases
        try
        {
            progressService.SetStatus(ArtistImportStatus.ImportingReleases);
            // Initialize total if not set already
            progressService.SetReleaseProgress(processed, totalEligible);
        }
        catch { }

        foreach (var rg in eligible)
        {
            try
            {
                var folderName = SanitizeFolderName(rg.Title ?? "");
                if (string.IsNullOrWhiteSpace(folderName))
                {
                    processed++;
                    try { progressService.SetReleaseProgress(processed, totalEligible); } catch { }
                    continue;
                }

                var releaseDir = Path.Combine(artistDir, folderName);
                var releaseJsonPath = Path.Combine(releaseDir, "release.json");
                if (File.Exists(releaseJsonPath))
                {
                    // already imported (possibly from audio present)
                    logger.LogDebug("[ImportRG] Release '{Title}' already present at {Path}", rg.Title, releaseJsonPath);
                    processed++;
                    try { progressService.SetReleaseProgress(processed, totalEligible); } catch { }
                    continue;
                }

                await ImportReleaseIfMissingAsync(
                    artistDir,
                    releaseDir,
                    rg.Id,
                    rg.Title,
                    rg.PrimaryType
                );
                if (File.Exists(releaseJsonPath))
                {
                    created++;
                    logger.LogInformation("[ImportRG] Created release.json for '{Title}' at {Path}", rg.Title, releaseJsonPath);
                }
                processed++;
                try { progressService.SetReleaseProgress(processed, totalEligible); } catch { }
            }
            catch
            {
                // ignore single RG failures
                processed++;
                try { progressService.SetReleaseProgress(processed, totalEligible); } catch { }
            }
        }

        logger.LogInformation("[ImportRG] Completed import of eligible release groups. Created {Count} new releases.", created);
        return created;
    }

    public async Task ImportReleaseIfMissingAsync(
        string artistDir,
        string releaseDir,
        string releaseGroupId,
        string? releaseTitle,
        string? primaryType
    )
    {
        var releaseJsonPath = Path.Combine(releaseDir, "release.json");
        if (File.Exists(releaseJsonPath))
        {
            // Also ensure audio file paths are populated if missing
            await EnsureAudioFilePathsAsync(releaseDir, releaseJsonPath);
            return;
        }

        var folderName = Path.GetFileName(releaseDir) ?? string.Empty;
        var built = await releaseJsonBuilder.BuildAsync(
            artistDir,
            releaseGroupId,
            folderName,
            releaseTitle,
            primaryType
        );
        if (built is null)
        {
            logger.LogWarning(
                "[ImportRelease] No suitable release selected for group {ReleaseGroupId}",
                releaseGroupId
            );
            return;
        }

        await writer.WriteReleaseAsync(
            Path.GetFileName(artistDir) ?? string.Empty,
            folderName,
            built
        );
        await EnsureAudioFilePathsAsync(releaseDir, releaseJsonPath);
        logger.LogDebug("[ImportRelease] Wrote release.json for '{Title}' at {Path}", built.Title, releaseJsonPath);
    }

    private static async Task EnsureAudioFilePathsAsync(string releaseDir, string releaseJsonPath)
    {
        try
        {
            var existingText = await File.ReadAllTextAsync(releaseJsonPath);
            var jsonRelease = JsonSerializer.Deserialize<JsonRelease>(
                existingText,
                GetJsonOptions()
            );
            if (jsonRelease?.Tracks == null || jsonRelease.Tracks.Count == 0)
            {
                return;
            }

            var audioFiles = Directory
                .GetFiles(releaseDir)
                .Where(f => AudioExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .Select(Path.GetFileName)
                .ToList();

            bool anyUpdated = false;
            foreach (var track in jsonRelease.Tracks)
            {
                if (!string.IsNullOrEmpty(track.AudioFilePath))
                    continue;

                var index = track.TrackNumber - 1;
                if (index >= 0 && index < audioFiles.Count)
                {
                    track.AudioFilePath = "./" + audioFiles[index];
                    anyUpdated = true;
                }
            }

            if (anyUpdated)
            {
                var updatedText = JsonSerializer.Serialize(jsonRelease, GetJsonOptions());
                await File.WriteAllTextAsync(releaseJsonPath, updatedText);
            }
        }
        catch
        {
            // ignore
        }
    }

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
