using System.Text.Json;
using System.Text.Json.Serialization;
using Hqub.Lastfm;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ServerLibrary.Utils;
using MusicGQL.Integration.MusicBrainz;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import.Services;

/// <summary>
/// Builds a JsonRelease from MusicBrainz data and optional local context (audio files, artist.json).
/// Centralizes all mapping/enrichment to be the single source of truth.
/// </summary>
public class ReleaseJsonBuilder(
    MusicBrainzService musicBrainzService,
    CoverArtDownloadService coverArtDownloadService,
    LastfmClient lastfmClient
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
        // Fetch candidate releases for the RG
        var releases = await musicBrainzService.GetReleasesForReleaseGroupAsync(releaseGroupId);

        // Evaluate local audio files (if any) to influence selection
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
        }

        // If release.json exists already, honor an explicit override if present
        Hqub.MusicBrainz.Entities.Release? selected = null;
        string? existingOverrideId = null;
        try
        {
            var existingPath = Path.Combine(releaseDir, "release.json");
            if (File.Exists(existingPath))
            {
                var txt = await File.ReadAllTextAsync(existingPath);
                var existing = JsonSerializer.Deserialize<JsonRelease>(txt, GetJsonOptions());
                var overrideId = existing?.Connections?.MusicBrainzReleaseIdOverride;
                if (!string.IsNullOrWhiteSpace(overrideId))
                {
                    existingOverrideId = overrideId;
                    var exact = await musicBrainzService.GetReleaseByIdAsync(overrideId!);
                    if (exact != null)
                    {
                        selected = exact;
                    }
                }
            }
        }
        catch { }

        // Select the best fitting release
        if (selected == null)
            if (audioFileCount <= 0)
            {
                selected = LibraryDecider.GetMainReleaseInReleaseGroup(releases.ToList());
            }
            else
            {
                // Prefer a release whose track count matches and whose titles best match filenames
                selected = releases
                    .OrderByDescending(r =>
                    {
                        var mediaTracks =
                            r.Media?.SelectMany(m =>
                                    m.Tracks ?? new List<Hqub.MusicBrainz.Entities.Track>()
                                )
                                .Count() ?? 0;
                        var exactMatch = mediaTracks == audioFileCount;
                        var isAlbum = string.Equals(
                            r.ReleaseGroup?.PrimaryType,
                            "Album",
                            StringComparison.OrdinalIgnoreCase
                        );
                        var score = 0;

                        if (exactMatch)
                            score += 10000;
                        if (isAlbum)
                            score += 1000;
                        if (string.Equals(r.Country, "US", StringComparison.OrdinalIgnoreCase))
                            score += 100;

                        // Penalize likely deluxe/anniversary editions by title
                        var title = r.Title ?? string.Empty;
                        if (
                            title.Contains("deluxe", StringComparison.OrdinalIgnoreCase)
                            || title.Contains("anniversary", StringComparison.OrdinalIgnoreCase)
                            || title.Contains("expanded", StringComparison.OrdinalIgnoreCase)
                            || title.Contains("remaster", StringComparison.OrdinalIgnoreCase)
                            || title.Contains("special", StringComparison.OrdinalIgnoreCase)
                            || title.Contains("bonus", StringComparison.OrdinalIgnoreCase)
                            || title.Contains("tour", StringComparison.OrdinalIgnoreCase)
                        )
                        {
                            score -= 500;
                        }

                        // Title/filename similarity
                        var titles =
                            r.Media?.SelectMany(m =>
                                    m.Tracks ?? new List<Hqub.MusicBrainz.Entities.Track>()
                                )
                                .Select(tt => tt.Recording?.Title ?? string.Empty)
                                .Where(s => !string.IsNullOrWhiteSpace(s))
                                .Select(Normalize)
                                .ToList() ?? new List<string>();

                        var filesNorm = audioFiles
                            .Select(f => Normalize(Path.GetFileNameWithoutExtension(f)))
                            .ToList();

                        int matchCount = 0;
                        int considered = Math.Min(Math.Min(titles.Count, filesNorm.Count), 30); // limit cost
                        for (int i = 0; i < considered; i++)
                        {
                            var t = titles[i];
                            var f = filesNorm[i];
                            if (t.Length == 0 || f.Length == 0)
                                continue;
                            if (
                                f.Contains(t, StringComparison.OrdinalIgnoreCase)
                                || t.Contains(f, StringComparison.OrdinalIgnoreCase)
                            )
                            {
                                matchCount++;
                            }
                            else
                            {
                                // loose word overlap (>=2 shared words)
                                var tw = t.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                var fw = f.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                var overlap = tw.Intersect(fw, StringComparer.OrdinalIgnoreCase)
                                    .Count();
                                if (overlap >= 2)
                                    matchCount++;
                            }
                        }
                        score += matchCount * 50;

                        // Finally, closeness of track count
                        score += Math.Max(0, 100 - Math.Abs(mediaTracks - audioFileCount));
                        return score;
                    })
                    .FirstOrDefault();
            }

        if (selected == null)
        {
            return null;
        }

        // Ensure target folder exists (for cover art download)
        if (!Directory.Exists(releaseDir))
        {
            Directory.CreateDirectory(releaseDir);
        }

        // Cover art
        string? coverArtRelPath = await coverArtDownloadService.DownloadReleaseCoverArtAsync(
            releaseGroupId,
            releaseDir
        );

        // Map release type
        var releaseType = primaryType?.ToLowerInvariant() switch
        {
            "album" => JsonReleaseType.Album,
            "ep" => JsonReleaseType.Ep,
            "single" => JsonReleaseType.Single,
            _ => JsonReleaseType.Album,
        };

        // Build a lookup of MusicBrainz artist IDs -> local artist Ids in server library
        var mbToLocalArtistId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var libraryRoot = Path.Combine("./", "Library");
            if (Directory.Exists(libraryRoot))
            {
                foreach (var artistPath in Directory.GetDirectories(libraryRoot))
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
            }
        }
        catch { }

        // Fetch recordings with credits to populate track credits
        var recordings = await musicBrainzService.GetRecordingsForReleaseAsync(selected.Id);
        var recById = recordings.ToDictionary(r => r.Id, r => r);

        var tracks = selected
            ?.Media?.SelectMany(m => m.Tracks ?? new List<Hqub.MusicBrainz.Entities.Track>())
            .Select(t =>
            {
                var recordingId = t.Recording?.Id;
                List<JsonTrackCredit>? trackCredits = null;
                if (
                    !string.IsNullOrWhiteSpace(recordingId)
                    && recById.TryGetValue(recordingId!, out var rec)
                )
                {
                    var recCredits =
                        rec.Credits ?? new List<Hqub.MusicBrainz.Entities.NameCredit>();
                    var list = new List<JsonTrackCredit>();
                    foreach (var c in recCredits)
                    {
                        var mbArtistId = c.Artist?.Id;
                        string? localArtistId = null;
                        if (
                            !string.IsNullOrWhiteSpace(mbArtistId)
                            && mbToLocalArtistId.TryGetValue(mbArtistId!, out var local)
                        )
                        {
                            localArtistId = local;
                        }
                        var artistName = !string.IsNullOrWhiteSpace(c.Name)
                            ? c.Name!
                            : c.Artist?.Name ?? string.Empty;
                        list.Add(
                            new JsonTrackCredit
                            {
                                ArtistName = artistName,
                                ArtistId = localArtistId,
                                MusicBrainzArtistId = mbArtistId,
                            }
                        );
                    }
                    trackCredits = list.Count > 0 ? list : null;
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

        // Enrich with Last.fm statistics (best effort)
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
                        }
                    }
                    catch { }
                }
            }
        }
        catch { }

        // Map audio file paths by track number if we have local files
        if (tracks != null && tracks.Count > 0 && audioFiles.Count > 0)
        {
            var fileNames = audioFiles.Select(Path.GetFileName).ToList();
            foreach (var track in tracks)
            {
                var index = track.TrackNumber - 1;
                if (index >= 0 && index < fileNames.Count)
                {
                    track.AudioFilePath = "./" + fileNames[index];
                }
            }
        }

        return new JsonRelease
        {
            Title = releaseTitle ?? releaseFolderName,
            SortTitle = releaseTitle,
            Type = releaseType,
            FirstReleaseDate = selected?.ReleaseGroup?.FirstReleaseDate,
            FirstReleaseYear =
                selected?.ReleaseGroup?.FirstReleaseDate?.Length >= 4
                    ? selected!.ReleaseGroup!.FirstReleaseDate!.Substring(0, 4)
                    : null,
            CoverArt = coverArtRelPath,
            Tracks = tracks?.Count > 0 ? tracks : null,
            Connections = new ReleaseServiceConnections
            {
                MusicBrainzReleaseGroupId = releaseGroupId,
                MusicBrainzSelectedReleaseId = selected.Id,
                // If an explicit override was used, keep it; otherwise leave null
                MusicBrainzReleaseIdOverride = existingOverrideId,
            },
        };
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
