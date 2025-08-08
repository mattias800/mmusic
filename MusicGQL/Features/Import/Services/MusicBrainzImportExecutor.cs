using System.Text.Json;
using System.Text.Json.Serialization;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Integration.MusicBrainz;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import.Services;

public interface IImportExecutor
{
    Task ImportArtistIfMissingAsync(string artistDir, string mbArtistId, string artistDisplayName);
    Task ImportReleaseIfMissingAsync(string artistDir, string releaseDir, string releaseGroupId, string? releaseTitle, string? primaryType);
}

public sealed class MusicBrainzImportExecutor(
    MusicBrainzService musicBrainzService,
    FanArtDownloadService fanArtDownloadService
) : IImportExecutor
{
    private static readonly string[] AudioExtensions = [".mp3", ".flac", ".wav", ".m4a", ".ogg"];

    public async Task ImportArtistIfMissingAsync(string artistDir, string mbArtistId, string artistDisplayName)
    {
        var artistJsonPath = Path.Combine(artistDir, "artist.json");
        if (File.Exists(artistJsonPath))
        {
            return;
        }

        var photos = await fanArtDownloadService.DownloadArtistPhotosAsync(mbArtistId, artistDir);

        var jsonArtist = new JsonArtist
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
            Connections = new JsonArtistServiceConnections
            {
                MusicBrainzArtistId = mbArtistId,
            },
        };

        var artistJson = JsonSerializer.Serialize(jsonArtist, GetJsonOptions());
        await File.WriteAllTextAsync(artistJsonPath, artistJson);
    }

    public async Task ImportReleaseIfMissingAsync(string artistDir, string releaseDir, string releaseGroupId, string? releaseTitle, string? primaryType)
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

        string? coverArtRelPath = await fanArtDownloadService.DownloadReleaseCoverArtAsync(releaseGroupId, releaseDir);

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
            FirstReleaseYear = selected?.ReleaseGroup?.FirstReleaseDate?.Length >= 4
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
            var jsonRelease = JsonSerializer.Deserialize<JsonRelease>(existingText, GetJsonOptions());
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


