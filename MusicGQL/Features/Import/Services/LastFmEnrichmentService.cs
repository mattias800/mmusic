using System.Text.Json;
using System.Text.Json.Serialization;
using Hqub.Lastfm;
using MusicGQL.Features.ServerLibrary.Json;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import.Services;

public class LastFmEnrichmentService(LastfmClient lastfmClient)
{
    private static JsonSerializerOptions GetJsonOptions() =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

    public async Task<EnrichmentResult> EnrichArtistAsync(string artistDir, string mbArtistId)
    {
        var result = new EnrichmentResult { ArtistDir = artistDir };
        var artistJsonPath = Path.Combine(artistDir, "artist.json");

        if (!File.Exists(artistJsonPath))
        {
            result.ErrorMessage = "artist.json not found";
            return result;
        }

        JsonArtist? jsonArtist;
        try
        {
            var text = await File.ReadAllTextAsync(artistJsonPath);
            jsonArtist = JsonSerializer.Deserialize<JsonArtist>(text, GetJsonOptions());
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Failed to read artist.json: {ex.Message}";
            return result;
        }

        if (jsonArtist == null)
        {
            result.ErrorMessage = "Malformed artist.json";
            return result;
        }

        try
        {
            var info = await lastfmClient.Artist.GetInfoByMbidAsync(mbArtistId);
            jsonArtist.MonthlyListeners = info?.Statistics?.Listeners;

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

            // Map to local library if present
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
                        if (topTrack.ReleaseFolderName != null && topTrack.TrackNumber != null)
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
                            // Fill release title when we can match to a release
                            topTrack.ReleaseTitle = releaseJson.Title;
                            // Use release cover art for portable cover art
                            if (!string.IsNullOrWhiteSpace(releaseJson.CoverArt))
                            {
                                topTrack.CoverArt = releaseJson.CoverArt;
                            }
                        }
                    }
                }
            }

            var updated = JsonSerializer.Serialize(jsonArtist, GetJsonOptions());
            await File.WriteAllTextAsync(artistJsonPath, updated);
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public class EnrichmentResult
    {
        public bool Success { get; set; }
        public string ArtistDir { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}
