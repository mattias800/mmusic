using System.Text.Json;
using System.Text.Json.Serialization;
using Hqub.Lastfm;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Features.ServerLibrary.Utils;
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
    FanArtDownloadService fanArtDownloadService,
    LastfmClient lastfmClient,
    Integration.Spotify.SpotifyService spotifyService,
    ILogger<MusicBrainzImportExecutor> logger
) : IImportExecutor
{
    private static readonly string[] AudioExtensions = [".mp3", ".flac", ".wav", ".m4a", ".ogg"];

    private static string NormalizeTitle(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var s = input
            .Replace("’", "'")
            .Replace("“", "\"")
            .Replace("”", "\"");
        var builder = new System.Text.StringBuilder(s.Length);
        foreach (var ch in s)
        {
            if (char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
        }
        var normalized = System.Text.RegularExpressions.Regex.Replace(builder.ToString(), "\\s+", " ").Trim();
        return normalized;
    }

    private static string StripParentheses(string input)
    {
        return System.Text.RegularExpressions.Regex.Replace(input, "\\(.*?\\)", string.Empty).Trim();
    }

    private static bool AreTitlesEquivalent(string a, string b)
    {
        var na = NormalizeTitle(a);
        var nb = NormalizeTitle(b);
        if (na.Equals(nb, StringComparison.Ordinal)) return true;
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
            var photos = await fanArtDownloadService.DownloadArtistPhotosAsync(
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

        var artistJson = JsonSerializer.Serialize(jsonArtist, GetJsonOptions());
        await File.WriteAllTextAsync(artistJsonPath, artistJson);
    }

    public async Task<int> ImportEligibleReleaseGroupsAsync(string artistDir, string mbArtistId)
    {
        var releaseGroups = await musicBrainzService.GetReleaseGroupsForArtistAsync(mbArtistId);
        int created = 0;

        foreach (var rg in releaseGroups)
        {
            try
            {
                if (!LibraryDecider.ShouldBeAddedWhenAddingArtistToServerLibrary(rg))
                {
                    continue;
                }

                var folderName = SanitizeFolderName(rg.Title ?? "");
                if (string.IsNullOrWhiteSpace(folderName))
                {
                    continue;
                }

                var releaseDir = Path.Combine(artistDir, folderName);
                var releaseJsonPath = Path.Combine(releaseDir, "release.json");
                if (File.Exists(releaseJsonPath))
                {
                    // already imported (possibly from audio present)
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
                }
            }
            catch
            {
                // ignore single RG failures
            }
        }

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

        var releases = await musicBrainzService.GetReleasesForReleaseGroupAsync(releaseGroupId);

        // Prefer a release whose track count matches local audio files if folder exists; otherwise assume none
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
        }

        logger.LogInformation(
            "[ImportRelease] Evaluating releases for group {ReleaseGroupId} in dir '{ReleaseDir}'. Audio files found: {AudioCount}. Files: {Files}",
            releaseGroupId,
            releaseDir,
            audioFileCount,
            string.Join(", ", audioFiles.Select(Path.GetFileName))
        );

        foreach (var r in releases)
        {
            var trackCount = r.Media?.SelectMany(m => m.Tracks ?? new List<Hqub.MusicBrainz.Entities.Track>()).Count() ?? 0;
            logger.LogInformation(
                "[ImportRelease] Candidate Release: Id={Id}, Title='{Title}', Date={Date}, Country={Country}, TrackCount={TrackCount}",
                r.Id,
                r.Title,
                r.Date,
                r.Country,
                trackCount
            );
        }

        Hqub.MusicBrainz.Entities.Release? selected;
        if (audioFileCount <= 0)
        {
            selected = LibraryDecider.GetMainReleaseInReleaseGroup(releases.ToList());
        }
        else
        {
            selected = releases
                .OrderByDescending(r =>
                {
                    var mediaTracks = r.Media?.SelectMany(m => m.Tracks ?? new List<Hqub.MusicBrainz.Entities.Track>()).Count() ?? 0;
                    var exactMatch = mediaTracks == audioFileCount;
                    var isAlbum = string.Equals(r.ReleaseGroup?.PrimaryType, "Album", StringComparison.OrdinalIgnoreCase);
                    var score = 0;
                    if (exactMatch) score += 10000;
                    if (isAlbum) score += 1000;
                    if (string.Equals(r.Country, "US", StringComparison.OrdinalIgnoreCase)) score += 100;
                    score += Math.Max(0, 100 - Math.Abs(mediaTracks - audioFileCount));
                    return score;
                })
                .FirstOrDefault();
        }

        if (selected == null)
        {
            logger.LogWarning("[ImportRelease] No suitable release selected for group {ReleaseGroupId}", releaseGroupId);
            return; // do not create folder or write release.json
        }

        // ensure folder exists only now that we know we will save a release.json
        if (!Directory.Exists(releaseDir))
        {
            Directory.CreateDirectory(releaseDir);
        }

        var selectedCount = selected.Media?.SelectMany(m => m.Tracks ?? new List<Hqub.MusicBrainz.Entities.Track>()).Count() ?? 0;
        logger.LogInformation(
            "[ImportRelease] Selected Release: Id={Id}, Title='{Title}', TrackCount={TrackCount}, AudioFiles={AudioCount}",
            selected.Id,
            selected.Title,
            selectedCount,
            audioFileCount
        );

        string? coverArtRelPath = await fanArtDownloadService.DownloadReleaseCoverArtAsync(
            releaseGroupId,
            releaseDir
        );

        var releaseType = primaryType?.ToLowerInvariant() switch
        {
            "album" => JsonReleaseType.Album,
            "ep" => JsonReleaseType.Ep,
            "single" => JsonReleaseType.Single,
            _ => JsonReleaseType.Album,
        };

        var tracks = selected
            ?.Media?.SelectMany(m => m.Tracks ?? new List<Hqub.MusicBrainz.Entities.Track>())
            .Select(t => new JsonTrack
            {
                Title = t.Recording?.Title ?? string.Empty,
                TrackNumber = t.Position,
                TrackLength = t.Length,
            })
            .Where(t => t.TrackNumber > 0)
            .OrderBy(t => t.TrackNumber)
            .ToList();

        var jsonRelease = new JsonRelease
        {
            Title = releaseTitle ?? Path.GetFileName(releaseDir) ?? string.Empty,
            SortTitle = releaseTitle,
            Type = releaseType,
            FirstReleaseDate = selected?.ReleaseGroup?.FirstReleaseDate,
            FirstReleaseYear =
                selected?.ReleaseGroup?.FirstReleaseDate?.Length >= 4
                    ? selected!.ReleaseGroup!.FirstReleaseDate!.Substring(0, 4)
                    : null,
            CoverArt = coverArtRelPath,
            Tracks = tracks?.Count > 0 ? tracks : null,
        };

        var jsonText = JsonSerializer.Serialize(jsonRelease, GetJsonOptions());
        await File.WriteAllTextAsync(releaseJsonPath, jsonText);

        await EnsureAudioFilePathsAsync(releaseDir, releaseJsonPath);
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
