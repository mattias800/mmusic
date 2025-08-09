using System.Text.Json;
using System.Text.Json.Serialization;
using Hqub.Lastfm;
using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ServerLibrary.Json;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import.Services;

/// <summary>
/// Service responsible for importing a single release group and writing release.json
/// Also enriches tracks with per-track Last.fm play count when available.
/// </summary>
public class LibraryReleaseImportService(
    MusicBrainzImportService musicBrainzService,
    FanArtDownloadService fanArtService,
    LastfmClient lastfmClient
)
{
    private static JsonSerializerOptions GetJsonOptions() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public async Task<SingleReleaseImportResult> ImportReleaseGroupAsync(
        MusicBrainzReleaseGroupResult releaseGroup,
        string artistFolderPath,
        string artistId
    )
    {
        var result = new SingleReleaseImportResult
        {
            ReleaseGroupId = releaseGroup.Id,
            Title = releaseGroup.Title,
        };

        // 1. Create release folder
        var releaseFolderName = SanitizeFolderName(releaseGroup.Title);
        var releaseFolderPath = Path.Combine(artistFolderPath, releaseFolderName);

        if (Directory.Exists(releaseFolderPath))
        {
            result.ErrorMessage = $"Release folder already exists: {releaseFolderName}";
            return result;
        }

        Directory.CreateDirectory(releaseFolderPath);

        try
        {
            // 2. Get releases with tracks
            var releases = await musicBrainzService.GetReleaseGroupReleasesAsync(releaseGroup.Id);

            if (!releases.Any())
            {
                result.ErrorMessage = "No releases found for release group";
                return result;
            }

            // Take the first release (or find the best one)
            var selectedRelease = releases.First();

            // 3. Download cover art
            var coverArtPath = await fanArtService.DownloadReleaseCoverArtAsync(
                releaseGroup.Id,
                releaseFolderPath
            );

            // 4. Create release.json
            var releaseType = releaseGroup.PrimaryType?.ToLowerInvariant() switch
            {
                "album" => JsonReleaseType.Album,
                "ep" => JsonReleaseType.Ep,
                "single" => JsonReleaseType.Single,
                _ => JsonReleaseType.Album,
            };

            // Fetch per-track Last.fm play counts
            var artistJsonPath = Path.Combine(artistFolderPath, "artist.json");
            string? artistName = null;
            try
            {
                if (File.Exists(artistJsonPath))
                {
                    var artistJsonText = await File.ReadAllTextAsync(artistJsonPath);
                    var jsonArtist = JsonSerializer.Deserialize<JsonArtist>(artistJsonText, GetJsonOptions());
                    artistName = jsonArtist?.Name;
                }
            }
            catch
            {
                // ignore errors reading artist.json; we'll proceed without artistName
            }

            var enrichedTracks = new List<JsonTrack>();
            foreach (var track in selectedRelease.Tracks)
            {
                long? playCount = null;
                if (!string.IsNullOrWhiteSpace(artistName))
                {
                    try
                    {
                        // Last.fm Track.GetInfo requires artist name and track title
                        var lfTrack = await lastfmClient.Track.GetInfoAsync(track.Title, artistName);
                        playCount = lfTrack?.Statistics?.PlayCount;
                    }
                    catch
                    {
                        // ignore Last.fm errors per track to keep import robust
                    }
                }

                enrichedTracks.Add(new JsonTrack
                {
                    Title = track.Title,
                    TrackNumber = track.TrackNumber,
                    TrackLength = track.Length,
                    PlayCount = playCount,
                    AudioFilePath = null,
                });
            }

            var releaseJson = new JsonRelease
            {
                Title = releaseGroup.Title,
                SortTitle = releaseGroup.Title,
                Type = releaseType,
                FirstReleaseDate = releaseGroup.FirstReleaseDate,
                CoverArt = coverArtPath,
                Tracks = enrichedTracks,
            };

            // 5. Write release.json file
            var releaseJsonPath = Path.Combine(releaseFolderPath, "release.json");
            var jsonOptions = GetJsonOptions();
            var jsonContent = JsonSerializer.Serialize(releaseJson, jsonOptions);
            await File.WriteAllTextAsync(releaseJsonPath, jsonContent);

            result.Success = true;
            result.ReleaseFolderPath = releaseFolderPath;
            result.ReleaseJson = releaseJson;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;

            // Cleanup on error
            if (Directory.Exists(releaseFolderPath))
            {
                try
                {
                    Directory.Delete(releaseFolderPath, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        return result;
    }

    private static string SanitizeFolderName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("", name.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Trim();
    }
}

