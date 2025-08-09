using System.Text.Json;
using System.Text.Json.Serialization;
using Hqub.Lastfm;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Features.ServerLibrary.Utils;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import.Services;

/// <summary>
/// Builds a JsonRelease from MusicBrainz data and optional local context (audio files, artist.json).
/// Centralizes all mapping/enrichment to be the single source of truth.
/// </summary>
public class ReleaseJsonBuilder(
    MusicBrainzService musicBrainzService,
    FanArtDownloadService fanArtDownloadService,
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

        // Select the best fitting release
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
            return null;
        }

        // Ensure target folder exists (for cover art download)
        if (!Directory.Exists(releaseDir))
        {
            Directory.CreateDirectory(releaseDir);
        }

        // Cover art
        string? coverArtRelPath = await fanArtDownloadService.DownloadReleaseCoverArtAsync(
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
                        if (!File.Exists(artistJsonPath)) continue;
                        var text = await File.ReadAllTextAsync(artistJsonPath);
                        var jsonArtist = JsonSerializer.Deserialize<JsonArtist>(text, GetJsonOptions());
                        var mbId = jsonArtist?.Connections?.MusicBrainzArtistId;
                        if (!string.IsNullOrWhiteSpace(mbId) && !string.IsNullOrWhiteSpace(jsonArtist?.Id))
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
                if (!string.IsNullOrWhiteSpace(recordingId) && recById.TryGetValue(recordingId!, out var rec))
                {
                    var recCredits = rec.Credits ?? new List<Hqub.MusicBrainz.Entities.NameCredit>();
                    var list = new List<JsonTrackCredit>();
                    foreach (var c in recCredits)
                    {
                        var mbArtistId = c.Artist?.Id;
                        string? localArtistId = null;
                        if (!string.IsNullOrWhiteSpace(mbArtistId) && mbToLocalArtistId.TryGetValue(mbArtistId!, out var local))
                        {
                            localArtistId = local;
                        }
                        var artistName = !string.IsNullOrWhiteSpace(c.Name)
                            ? c.Name!
                            : c.Artist?.Name ?? string.Empty;
                        list.Add(new JsonTrackCredit
                        {
                            ArtistName = artistName,
                            ArtistId = localArtistId,
                            MusicBrainzArtistId = mbArtistId,
                        });
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
                        var info = await lastfmClient.Track.GetInfoAsync(jt.Title, artistDisplayName);
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
            FirstReleaseYear = selected?.ReleaseGroup?.FirstReleaseDate?.Length >= 4
                ? selected!.ReleaseGroup!.FirstReleaseDate!.Substring(0, 4)
                : null,
            CoverArt = coverArtRelPath,
            Tracks = tracks?.Count > 0 ? tracks : null,
        };
    }

    private static JsonSerializerOptions GetJsonOptions() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };
}


