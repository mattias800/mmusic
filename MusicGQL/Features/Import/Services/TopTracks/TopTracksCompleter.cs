using Hqub.Lastfm;
using Microsoft.Extensions.Logging;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Integration.Spotify;

namespace MusicGQL.Features.Import.Services.TopTracks;

public class TopTracksCompleter(SpotifyService spotifyService, LastfmClient lastfmClient, ILogger logger)
{
    private static string Normalize(string s) => (s ?? string.Empty).Trim().ToLowerInvariant();

    public async Task CompleteAsync(string artistDir, JsonArtist artist)
    {
        const int MaxTop = 25;
        // Weights are tuned to favor Spotify popularity when Last.fm scrobbles are small or missing
        const double Wlf = 0.5; // weight for Last.fm (log10 scale)
        const double Wsp = 2.0; // weight for Spotify popularity (0..1)
        const long LfIgnoreBelow = 10000; // if LF plays < this, we effectively ignore LF for ranking
        if (artist.TopTracks == null || artist.TopTracks.Count == 0)
        {
            return;
        }

        // Build candidate artist names for Last.fm lookups (handle Ye/Kanye aliasing)
        var candidateArtistNames = new List<string>();
        void addName(string? s)
        {
            if (!string.IsNullOrWhiteSpace(s) && !candidateArtistNames.Contains(s, StringComparer.OrdinalIgnoreCase))
                candidateArtistNames.Add(s);
        }
        addName(artist.Name);
        addName(artist.SortName);
        if (artist.Aliases != null)
        {
            foreach (var a in artist.Aliases)
            {
                addName(a.Name);
                addName(a.SortName);
            }
        }

        // Build a map of Spotify tracks across all linked Spotify IDs (and legacy),
        // choosing the entry with the highest popularity for each normalized title.
        var allSpotifyIds = new List<string>();
        if (artist.Connections?.SpotifyIds != null)
        {
            allSpotifyIds.AddRange(artist.Connections.SpotifyIds.Select(s => s.Id));
        }
        if (!string.IsNullOrWhiteSpace(artist.Connections?.SpotifyId))
        {
            if (!allSpotifyIds.Contains(artist.Connections.SpotifyId, StringComparer.OrdinalIgnoreCase))
                allSpotifyIds.Add(artist.Connections.SpotifyId);
        }
        if (allSpotifyIds.Count == 0)
        {
            // Try to resolve one by name as a last resort
            try
            {
                var candidates = await spotifyService.SearchArtistsAsync(artist.Name, 1);
                var best = candidates?.FirstOrDefault();
                if (best != null)
                {
                    allSpotifyIds.Add(best.Id);
                    artist.Connections ??= new JsonArtistServiceConnections();
                    artist.Connections.SpotifyId = best.Id;
                }
            }
            catch { }
        }

        var titleToSpotify = new Dictionary<string, SpotifyAPI.Web.FullTrack>(StringComparer.OrdinalIgnoreCase);
        foreach (var sid in allSpotifyIds)
        {
            try
            {
                var list = await spotifyService.GetArtistTopTracksAsync(sid) ?? new List<SpotifyAPI.Web.FullTrack>();
                foreach (var t in list)
                {
                    var key = Normalize(t.Name);
                    if (titleToSpotify.TryGetValue(key, out var existing))
                    {
                        if (t.Popularity > existing.Popularity)
                        {
                            titleToSpotify[key] = t;
                        }
                    }
                    else
                    {
                        titleToSpotify[key] = t;
                    }
                }
            }
            catch { }
        }

        // Ensure an HttpClient for downloading images when needed
        using var httpClient = new HttpClient();

        for (int i = 0; i < artist.TopTracks.Count; i++)
        {
            var tt = artist.TopTracks[i];
            long? beforePlayCount = tt.PlayCount;

            var normalizedTitle = Normalize(tt.Title);
            titleToSpotify.TryGetValue(normalizedTitle, out var spotifyMatch);

            // Fallback release title from Spotify if missing
            if (string.IsNullOrWhiteSpace(tt.ReleaseTitle) && spotifyMatch?.Album != null)
            {
                tt.ReleaseTitle = spotifyMatch.Album.Name;
            }

            // Fill missing track length from Spotify if available
            if ((tt.TrackLength == null || tt.TrackLength == 0) && spotifyMatch != null)
            {
                tt.TrackLength = spotifyMatch.DurationMs;
            }

            // Check if this track already has real play count data (e.g., from ListenBrainz)
            bool hasRealPlayCount = tt.PlayCount.HasValue && tt.PlayCount.Value > 0;
            
            // Fill play count, preferring Last.fm when available, else Spotify popularity as proxy
            // BUT only if we don't already have real play count data
            long? lfCountUsed = null;
            int? spPopularityUsed = null;
            if (!hasRealPlayCount && tt.PlayCount == null)
            {
                bool set = false;
                foreach (var candidateArtist in candidateArtistNames)
                {
                    try
                    {
                        var lf = await lastfmClient.Track.GetInfoAsync(tt.Title, candidateArtist);
                        var lfPlays = lf?.Statistics?.PlayCount;
                        if (lfPlays != null)
                        {
                            tt.PlayCount = lfPlays;
                            lfCountUsed = lfPlays;
                            try { logger.LogInformation("[TopTracksCompleter] LF match title='{Title}' artist='{Artist}' plays={Plays} listeners={Listeners}", tt.Title, candidateArtist, lfPlays ?? 0, lf?.Statistics?.Listeners ?? 0); } catch { }
                            set = true;
                            break;
                        }
                    }
                    catch { }
                }

                double? spNorm = null;
                if (!set && spotifyMatch != null)
                {
                    // Popularity is 0-100; store as long for consistent ranking
                    spNorm = spotifyMatch.Popularity;
                    spPopularityUsed = spotifyMatch.Popularity;
                    tt.PlayCount = (long)Math.Round(spNorm.Value);
                    try { logger.LogInformation("[TopTracksCompleter] SP popularity match title='{Title}' popularity={Pop}", tt.Title, spPopularityUsed ?? -1); } catch { }
                }
            }

            // Compute RankScore using both signals when available
            // If we have real play count data, use it directly for ranking
            double lfScore = 0.0;
            if (hasRealPlayCount)
            {
                // Use the real play count for ranking (e.g., from ListenBrainz)
                lfScore = Math.Log10(tt.PlayCount.Value + 1.0);
                lfCountUsed = tt.PlayCount.Value;
                tt.RankSource = "listenbrainz"; // Mark as ListenBrainz data
            }
            else if (lfCountUsed.HasValue)
            {
                if (lfCountUsed.Value >= LfIgnoreBelow)
                {
                    lfScore = Math.Log10(lfCountUsed.Value + 1.0);
                }
                else
                {
                    // treat as effectively zero to avoid penalizing popular Spotify tracks with low LF scrobbles
                    lfScore = 0.0;
                }
            }
            double spScore = spPopularityUsed.HasValue ? (spPopularityUsed.Value / 100.0) : 0.0;
            var rankScore = Wlf * lfScore + Wsp * spScore;
            tt.RankScore = rankScore;
            
            // Set rank source based on what we used
            if (tt.RankSource == null)
            {
                tt.RankSource = lfCountUsed.HasValue && spPopularityUsed.HasValue ? "lf+sp"
                    : lfCountUsed.HasValue ? "lf"
                    : spPopularityUsed.HasValue ? "sp_popularity" : null;
            }

            // Log enrichment details for diagnostics
            try
            {
                if (lfCountUsed == null && spPopularityUsed == null)
                {
                    logger.LogInformation("[TopTracksCompleter] No LF/SP data for title='{Title}' (before={Before}, after={After})", tt.Title, beforePlayCount ?? 0, tt.PlayCount ?? 0);
                }
                else
                {
                    logger.LogInformation("[TopTracksCompleter] Track='{Title}' before={Before} after={After} lfUsed={Lf} spPop={Sp}", tt.Title, beforePlayCount ?? 0, tt.PlayCount ?? 0, lfCountUsed ?? 0, spPopularityUsed ?? -1);
                }
            }
            catch { }

            // Cover art handling
            if (string.IsNullOrWhiteSpace(tt.CoverArt))
            {
                logger.LogInformation("[TopTracksCompleter] Processing cover art for track '{Title}' (source: {Source})", tt.Title, tt.RankSource ?? "unknown");
                
                // If mapped to a local release, this should already have been set by caller from release cover art.
                // If still missing, try to get cover art from various sources
                string? coverArtUrl = null;
                string? coverArtSource = null;
                
                // Try to get cover art from ListenBrainz data if available
                // ListenBrainz has caa_id and caa_release_mbid which can be used to get cover art
                if (tt.RankSource == "listenbrainz")
                {
                    logger.LogInformation("[TopTracksCompleter] ListenBrainz track '{Title}' - attempting to get cover art from ListenBrainz data", tt.Title);
                    // TODO: Implement ListenBrainz cover art fetching using caa_id or caa_release_mbid
                    // For now, we'll fall back to Spotify
                    logger.LogInformation("[TopTracksCompleter] ListenBrainz cover art not yet implemented, falling back to Spotify", tt.Title);
                }
                
                // Try Spotify album image as fallback
                if (string.IsNullOrEmpty(coverArtUrl))
                {
                    var image = spotifyMatch?.Album?.Images?.FirstOrDefault();
                    if (image != null && !string.IsNullOrWhiteSpace(image.Url))
                    {
                        coverArtUrl = image.Url;
                        coverArtSource = "Spotify";
                        logger.LogInformation("[TopTracksCompleter] Found Spotify cover art for '{Title}': {Url}", tt.Title, image.Url);
                    }
                    else
                    {
                        logger.LogInformation("[TopTracksCompleter] No Spotify cover art available for '{Title}'", tt.Title);
                    }
                }
                
                // Download and save cover art if we found a URL
                if (!string.IsNullOrEmpty(coverArtUrl))
                {
                    logger.LogInformation("[TopTracksCompleter] Downloading cover art for '{Title}' from {Source}: {Url}", tt.Title, coverArtSource, coverArtUrl);
                    try
                    {
                        var bytes = await httpClient.GetByteArrayAsync(coverArtUrl);
                        var fileName = $"toptrack{(i + 1).ToString("00")}.jpg";
                        var fullPath = System.IO.Path.Combine(artistDir, fileName);
                        
                        logger.LogInformation("[TopTracksCompleter] Saving cover art for '{Title}' to {Path} ({Size} bytes)", tt.Title, fullPath, bytes.Length);
                        
                        await File.WriteAllBytesAsync(fullPath, bytes);
                        tt.CoverArt = "./" + fileName;
                        
                        logger.LogInformation("[TopTracksCompleter] Successfully downloaded and saved cover art for '{Title}': {FileName} ({Size} bytes)", 
                            tt.Title, fileName, bytes.Length);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "[TopTracksCompleter] Failed to download cover art for '{Title}' from {Url}", tt.Title, coverArtUrl);
                        // ignore download failures
                    }
                }
                else
                {
                    logger.LogInformation("[TopTracksCompleter] No cover art available for '{Title}' (source: {Source}) - will remain null", tt.Title, tt.RankSource ?? "unknown");
                }
            }
            else
            {
                logger.LogInformation("[TopTracksCompleter] Track '{Title}' already has cover art: {CoverArt}", tt.Title, tt.CoverArt);
            }
        }

        // Resort by rank score (then play count) now that we have enriched counts
        artist.TopTracks = artist.TopTracks
            .OrderByDescending(t => t.RankScore ?? 0)
            .ThenByDescending(t => t.PlayCount ?? 0)
            .ThenBy(t => t.Title)
            .Take(MaxTop)
            .ToList();

        // Summary log of final ranking
        try
        {
            for (int i = 0; i < Math.Min(10, artist.TopTracks.Count); i++)
            {
                var t = artist.TopTracks[i];
                logger.LogInformation("[TopTracksCompleter] FinalRank #{Rank}: '{Title}' score={Score:F3} plays={Plays} source={Source}", i + 1, t.Title, t.RankScore ?? 0, t.PlayCount ?? 0, t.RankSource ?? "");
            }
        }
        catch { }
    }
}
