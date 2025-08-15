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

        // Build candidate names from artist name, sortName, and aliases
        var candidateNames = new List<string>();
        void add(string? s)
        {
            if (!string.IsNullOrWhiteSpace(s) && !candidateNames.Contains(s, StringComparer.OrdinalIgnoreCase))
                candidateNames.Add(s);
        }
        add(jsonArtist.Name);
        add(jsonArtist.SortName);
        if (jsonArtist.Aliases != null)
        {
            foreach (var a in jsonArtist.Aliases)
            {
                add(a.Name);
                add(a.SortName);
            }
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
            // Try resolving by alias names
            foreach (var name in candidateNames)
            {
                try
                {
                    var infoByName = await lastfmClient.Artist.GetInfoAsync(name);
                    if (infoByName != null)
                    {
                        jsonArtist.MonthlyListeners = infoByName.Statistics?.Listeners;
                        logger.LogInformation("[EnrichArtist] Resolved Last.fm listeners via alias '{Alias}'", name);
                        break;
                    }
                }
                catch { }
            }
        }

        // Top tracks via Spotify first; fallback to Last.fm if Spotify fails or returns empty
        try
        {
            // ensure we have a spotify artist id
            var spotifyId = jsonArtist.Connections?.SpotifyId;
            if (string.IsNullOrWhiteSpace(spotifyId))
            {
                foreach (var name in candidateNames)
                {
                    try
                    {
                        var candidates = await spotifyService.SearchArtistsAsync(name, 3);
                        var best = candidates?.FirstOrDefault();
                        if (best != null)
                        {
                            jsonArtist.Connections ??= new JsonArtistServiceConnections();
                            jsonArtist.Connections.SpotifyId = best.Id; // legacy field
                            jsonArtist.Connections.SpotifyIds ??= new List<JsonSpotifyArtistIdentity>();
                            if (!jsonArtist.Connections.SpotifyIds.Any(x => string.Equals(x.Id, best.Id, StringComparison.OrdinalIgnoreCase)))
                            {
                                jsonArtist.Connections.SpotifyIds.Add(new JsonSpotifyArtistIdentity
                                {
                                    Id = best.Id,
                                    DisplayName = best.Name,
                                    Source = "search",
                                    AddedAt = DateTime.UtcNow.ToString("yyyy-MM-dd")
                                });
                            }
                            spotifyId = best.Id;
                            logger.LogInformation("[EnrichArtist] Resolved Spotify artist via name '{Name}'", name);
                            break;
                        }
                    }
                    catch { }
                }
            }

            // Collect all linked Spotify IDs (including legacy single field) and merge top tracks
            var allSpotifyIds = jsonArtist.Connections?.SpotifyIds?.Select(s => s.Id).ToList() ?? new List<string>();
            if (!string.IsNullOrWhiteSpace(spotifyId) && !allSpotifyIds.Contains(spotifyId))
            {
                allSpotifyIds.Add(spotifyId);
            }

            if (allSpotifyIds.Count > 0)
            {
                var spImporter = new TopTracks.TopTracksSpotifyImporter(spotifyService);
                var merged = new Dictionary<string, JsonTopTrack>(StringComparer.OrdinalIgnoreCase);
                foreach (var sid in allSpotifyIds)
                {
                    try
                    {
                        var tracks = await spImporter.GetTopTracksAsync(sid, 10) ?? new List<JsonTopTrack>();
                        foreach (var t in tracks)
                        {
                            var key = NormalizeTitle(t.Title);
                            if (merged.TryGetValue(key, out var existing))
                            {
                                if ((t.PlayCount ?? 0) > (existing.PlayCount ?? 0))
                                    merged[key] = t;
                            }
                            else
                            {
                                merged[key] = t;
                            }
                        }
                    }
                    catch { }
                }
                jsonArtist.TopTracks = merged.Values
                    .OrderByDescending(t => t.PlayCount ?? 0)
                    .Take(10)
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[EnrichArtist] Spotify top tracks failed for artist='{Name}'. Will attempt Last.fm fallback.", jsonArtist.Name);
            jsonArtist.TopTracks = [];
        }

        // Last.fm fallback for top tracks
        if (jsonArtist.TopTracks == null || jsonArtist.TopTracks.Count == 0)
        {
            try
            {
                TopTracks.ITopTracksImporter importer = new TopTracks.TopTracksLastFmImporter(lastfmClient);
                jsonArtist.TopTracks = await importer.GetTopTracksAsync(mbArtistId, 10);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "[EnrichArtist] Last.fm fallback for top tracks failed for mbid='{MbId}'",
                    mbArtistId);
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