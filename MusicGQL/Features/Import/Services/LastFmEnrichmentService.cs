using System.Text.Json;
using System.Text.Json.Serialization;
using Hqub.Lastfm;
using Microsoft.Extensions.Logging;
using MusicGQL.Features.Import.Services.TopTracks;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Integration.Spotify;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import.Services;

public class LastFmEnrichmentService(
    SpotifyService spotifyService,
    LastfmClient lastfmClient,
    TopTracksServiceManager topTracksServiceManager,
    ILogger<LastFmEnrichmentService> logger
)
{
    private static string NormalizeTitle(string s) => (s ?? string.Empty).Trim().ToLowerInvariant();

    public async Task EnrichArtistAsync(string artistDir, string mbArtistId)
    {
        var artistJsonPath = Path.Combine(artistDir, "artist.json");
        if (!File.Exists(artistJsonPath))
        {
            logger.LogWarning("[EnrichArtist] No artist.json found in {ArtistDir}", artistDir);
            return;
        }

        JsonArtist? jsonArtist;
        try
        {
            var artistText = await File.ReadAllTextAsync(artistJsonPath);
            jsonArtist = JsonSerializer.Deserialize<JsonArtist>(artistText, GetJsonOptions());
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[EnrichArtist] Failed to deserialize artist.json from {ArtistDir}",
                artistDir
            );
            return;
        }

        if (jsonArtist == null)
        {
            logger.LogWarning(
                "[EnrichArtist] Failed to deserialize artist.json from {ArtistDir}",
                artistDir
            );
            return;
        }

        // Get candidate names for external service lookups
        var candidateNames = new List<string>();
        void addName(string? s)
        {
            if (
                !string.IsNullOrWhiteSpace(s)
                && !candidateNames.Contains(s, StringComparer.OrdinalIgnoreCase)
            )
                candidateNames.Add(s);
        }
        addName(jsonArtist.Name);
        addName(jsonArtist.SortName);
        if (jsonArtist.Aliases != null)
        {
            foreach (var a in jsonArtist.Aliases)
            {
                addName(a.Name);
                addName(a.SortName);
            }
        }

        // TOP TRACKS: Use the new TopTracksServiceManager
        try
        {
            logger.LogInformation(
                "[EnrichArtist] Getting top tracks for artist '{Name}' using service manager",
                jsonArtist.Name
            );
            var topTracksResult = await topTracksServiceManager.GetTopTracksAsync(
                mbArtistId,
                jsonArtist.Name,
                25
            );

            if (topTracksResult.Success && topTracksResult.Tracks.Count > 0)
            {
                logger.LogInformation(
                    "[EnrichArtist] Got {Count} top tracks from {Source} for artist '{Name}'",
                    topTracksResult.Tracks.Count,
                    topTracksResult.Source,
                    jsonArtist.Name
                );
                jsonArtist.TopTracks = topTracksResult.Tracks;
            }
            else
            {
                // Preserve existing top tracks if available
                if (jsonArtist.TopTracks != null && jsonArtist.TopTracks.Count > 0)
                {
                    logger.LogInformation(
                        "[EnrichArtist] Preserving existing {Count} top tracks for artist '{Name}'",
                        jsonArtist.TopTracks.Count,
                        jsonArtist.Name
                    );
                }
                else
                {
                    logger.LogWarning(
                        "[EnrichArtist] No top tracks found for artist '{Name}'. Warnings: {Warnings}",
                        jsonArtist.Name,
                        string.Join("; ", topTracksResult.Warnings)
                    );
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[EnrichArtist] Failed to get top tracks for artist '{Name}'",
                jsonArtist.Name
            );
            // Preserve existing top tracks if available
            if (jsonArtist.TopTracks != null && jsonArtist.TopTracks.Count > 0)
            {
                logger.LogInformation(
                    "[EnrichArtist] Preserving existing {Count} top tracks for artist '{Name}' after error",
                    jsonArtist.TopTracks.Count,
                    jsonArtist.Name
                );
            }
        }

        // Complete missing fields using Spotify as fallback (releaseTitle, cover art download)
        try
        {
            var completer = new TopTracksCompleter(spotifyService, lastfmClient, logger);
            await completer.CompleteAsync(artistDir, jsonArtist);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "[EnrichArtist] TopTracks completer failed for artist='{Name}'",
                jsonArtist.Name
            );
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

                    if (releaseJson is null)
                        continue;

                    var folderName = Path.GetFileName(releaseDir) ?? string.Empty;

                    // Build a candidate list of tracks from both flattened Tracks and Discs
                    var candidateTracks = new List<(JsonTrack track, int discNumber)>();
                    if (releaseJson.Tracks != null)
                    {
                        foreach (var t in releaseJson.Tracks)
                        {
                            candidateTracks.Add((t, t.DiscNumber ?? 1));
                        }
                    }
                    if (releaseJson.Discs != null)
                    {
                        foreach (var disc in releaseJson.Discs)
                        {
                            var dnum = disc.DiscNumber <= 0 ? 1 : disc.DiscNumber;
                            if (disc.Tracks != null)
                            {
                                foreach (var t in disc.Tracks)
                                {
                                    candidateTracks.Add((t, t.DiscNumber ?? dnum));
                                }
                            }
                        }
                    }

                    foreach (var topTrack in jsonArtist.TopTracks)
                    {
                        if (topTrack.ReleaseFolderName != null && topTrack.TrackNumber != null)
                            continue;

                        // Find all title matches
                        var titleMatches = candidateTracks
                            .Where(ct =>
                                !string.IsNullOrWhiteSpace(ct.track.Title)
                                && AreTitlesEquivalent(ct.track.Title, topTrack.Title)
                            )
                            .ToList();

                        if (titleMatches.Count == 0)
                            continue;

                        // Prefer a match that has an assigned audio file present on disk
                        (JsonTrack track, int discNumber)? best = null;
                        foreach (var ct in titleMatches)
                        {
                            var audioRef = ct.track.AudioFilePath;
                            bool hasAudio = false;
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(audioRef))
                                {
                                    var rel = audioRef!;
                                    if (rel.StartsWith("./"))
                                        rel = rel[2..];
                                    var full = Path.Combine(releaseDir, rel);
                                    hasAudio = File.Exists(full);
                                }
                            }
                            catch { }

                            if (best is null)
                            {
                                best = ct;
                                if (hasAudio)
                                {
                                    // First good audio-backed match, take it
                                    break;
                                }
                            }
                            else if (hasAudio)
                            {
                                // Prefer an audio-backed match over a previous non-audio match
                                best = ct;
                                break;
                            }
                        }

                        if (best is { } chosen)
                        {
                            topTrack.ReleaseFolderName = folderName;
                            topTrack.TrackNumber = chosen.track.TrackNumber;
                            topTrack.ReleaseTitle = releaseJson.Title;

                            // Prefer release cover art if available
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
            logger.LogWarning(
                ex,
                "[EnrichArtist] Failed to map top tracks to local library for artist='{Name}'",
                jsonArtist.Name
            );
        }

        // Save updated artist.json
        try
        {
            var updatedJson = JsonSerializer.Serialize(jsonArtist, GetJsonOptions());
            await File.WriteAllTextAsync(artistJsonPath, updatedJson);
            logger.LogInformation(
                "[EnrichArtist] Updated artist.json for '{Name}'",
                jsonArtist.Name
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[EnrichArtist] Failed to save updated artist.json for '{Name}'",
                jsonArtist.Name
            );
        }
    }

    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };
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

        // Fallback: ignore parenthetical qualifiers
        var npa = NormalizeTitle(StripParentheses(a));
        var npb = NormalizeTitle(StripParentheses(b));
        return npa.Equals(npb, StringComparison.Ordinal);
    }
}
