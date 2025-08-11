using System.Text.Json;
using System.Text.Json.Serialization;
using Hqub.Lastfm;
using MusicGQL.Features.ServerLibrary.Json;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import.Services;

public class LastFmEnrichmentService(
    LastfmClient lastfmClient,
    Integration.Spotify.SpotifyService spotifyService,
    ILogger<LastFmEnrichmentService> logger
)
{
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

        // Fetch listeners from Last.fm (best effort)
        try
        {
            var info = await lastfmClient.Artist.GetInfoByMbidAsync(mbArtistId);
            jsonArtist.MonthlyListeners = info?.Statistics?.Listeners;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[EnrichArtist] Failed to fetch Last.fm artist info for mbid='{MbId}'", mbArtistId);
        }

        // Top tracks via Last.fm; fallback to Spotify if Last.fm fails or returns empty
        try
        {
            TopTracks.ITopTracksImporter importer = new TopTracks.TopTracksLastFmImporter(lastfmClient);
            jsonArtist.TopTracks = await importer.GetTopTracksAsync(mbArtistId, 10);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "[EnrichArtist] Last.fm top tracks failed for mbid='{MbId}'. Will attempt Spotify fallback.",
                mbArtistId);
            jsonArtist.TopTracks = [];
        }

        // Spotify fallback for top tracks
        if (jsonArtist.TopTracks == null || jsonArtist.TopTracks.Count == 0)
        {
            try
            {
                // ensure we have a spotify artist id
                var spotifyId = jsonArtist.Connections?.SpotifyId;
                if (string.IsNullOrWhiteSpace(spotifyId))
                {
                    var name = jsonArtist.Name;
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        var candidates = await spotifyService.SearchArtistsAsync(name, 1);
                        var best = candidates?.FirstOrDefault();
                        if (best != null)
                        {
                            jsonArtist.Connections ??= new JsonArtistServiceConnections();
                            jsonArtist.Connections.SpotifyId = best.Id;
                            spotifyId = best.Id;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(spotifyId))
                {
                    var spImporter = new TopTracks.TopTracksSpotifyImporter(spotifyService);
                    jsonArtist.TopTracks = await spImporter.GetTopTracksAsync(spotifyId!, 10);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[EnrichArtist] Spotify fallback for top tracks failed for artist='{Name}'",
                    jsonArtist.Name);
            }
        }

        // Complete missing fields using Spotify as fallback (releaseTitle, cover art download)
        try
        {
            var completer = new TopTracks.TopTracksCompleter(spotifyService);
            await completer.CompleteAsync(artistDir, jsonArtist);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[EnrichArtist] TopTracks completer failed for artist='{Name}'", jsonArtist.Name);
        }

        // Map to local library if present
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
                        if (topTrack.ReleaseFolderName != null && topTrack.TrackNumber != null)
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
                                var combined = Path.Combine(folderName, relPath).Replace('\\', '/');
                                topTrack.CoverArt = "./" + combined;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[EnrichArtist] Mapping top tracks to library failed for artist='{Name}'",
                jsonArtist.Name);
        }

        try
        {
            var updated = JsonSerializer.Serialize(jsonArtist, GetJsonOptions());
            await File.WriteAllTextAsync(artistJsonPath, updated);
            result.Success = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[EnrichArtist] Failed to write artist.json at '{Path}'", artistJsonPath);
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

        // Fallback: ignore parenthetical qualifiers
        var npa = NormalizeTitle(StripParentheses(a));
        var npb = NormalizeTitle(StripParentheses(b));
        return npa.Equals(npb, StringComparison.Ordinal);
    }

    private static JsonSerializerOptions GetJsonOptions() =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };
}