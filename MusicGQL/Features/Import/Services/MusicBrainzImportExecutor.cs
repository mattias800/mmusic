using System.Text.Json;
using System.Text.Json.Serialization;
using Hqub.Lastfm;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Integration.MusicBrainz;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import.Services;

public interface IImportExecutor
{
    Task ImportArtistIfMissingAsync(string artistDir, string mbArtistId, string artistDisplayName);

    Task ImportReleaseIfMissingAsync(
        string artistDir,
        string releaseDir,
        string releaseGroupId,
        string? releaseTitle,
        string? primaryType
    );
}

public sealed class MusicBrainzImportExecutor(
    MusicBrainzService musicBrainzService,
    FanArtDownloadService fanArtDownloadService,
    LastfmClient lastfmClient
) : IImportExecutor
{
    private static readonly string[] AudioExtensions = [".mp3", ".flac", ".wav", ".m4a", ".ogg"];

    public async Task ImportArtistIfMissingAsync(
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

                var top = await lastfmClient.Artist.GetTopTracksByMbidAsync(mbArtistId);
                if (top != null)
                {
                    jsonArtist.TopTracks = top.Take(10)
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
                                    && string.Equals(
                                        t.Title,
                                        topTrack.Title,
                                        StringComparison.OrdinalIgnoreCase
                                    )
                                );

                                if (match != null)
                                {
                                    topTrack.ReleaseFolderName = folderName;
                                    topTrack.TrackNumber = match.TrackNumber;
                                    topTrack.ReleaseTitle = releaseJson.Title;
                                    if (!string.IsNullOrWhiteSpace(releaseJson.CoverArt))
                                    {
                                        topTrack.CoverArt = releaseJson.CoverArt;
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
            }
        }
        catch
        {
            // ignore Last.fm failures
        }

        var artistJson = JsonSerializer.Serialize(jsonArtist, GetJsonOptions());
        await File.WriteAllTextAsync(artistJsonPath, artistJson);
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
        var selected = releases.FirstOrDefault();

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
