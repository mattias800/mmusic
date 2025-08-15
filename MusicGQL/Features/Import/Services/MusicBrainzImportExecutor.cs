using System.Text.Json;
using System.Text.Json.Serialization;
using Hqub.Lastfm;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ServerLibrary.Utils;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Features.ArtistImportQueue;
using MusicGQL.Features.ArtistImportQueue.Services;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import.Services;

public interface IImportExecutor
{
    Task ImportOrEnrichArtistAsync(string artistDir, string mbArtistId, string artistDisplayName);

    Task ImportReleaseIfMissingAsync(
        string artistDir,
        string releaseDir,
        string releaseGroupId,
        string? releaseTitle,
        string? primaryType
    );

    Task<int> ImportEligibleReleaseGroupsAsync(string artistDir, string mbArtistId);
}

public sealed class MusicBrainzImportExecutor(
    MusicBrainzService musicBrainzService,
    CoverArtDownloadService coverArtDownloadService,
    LastfmClient lastfmClient,
    Integration.Spotify.SpotifyService spotifyService,
    ILogger<MusicBrainzImportExecutor> logger,
    ReleaseJsonBuilder releaseJsonBuilder,
    ServerLibrary.Writer.ServerLibraryJsonWriter writer,
    CurrentArtistImportStateService progressService
) : IImportExecutor
{
    private static readonly string[] AudioExtensions = [".mp3", ".flac", ".wav", ".m4a", ".ogg"];

    private static string NormalizeTitle(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        var s = input.Replace("’", "'").Replace("“", "\"").Replace("”", "\"");
        var builder = new System.Text.StringBuilder(s.Length);
        foreach (var ch in s)
        {
            if (char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
        }

        var normalized = System
            .Text.RegularExpressions.Regex.Replace(builder.ToString(), "\\s+", " ")
            .Trim();
        return normalized;
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
        var npa = NormalizeTitle(StripParentheses(a));
        var npb = NormalizeTitle(StripParentheses(b));
        return npa.Equals(npb, StringComparison.Ordinal);
    }

    private static string SanitizeFolderName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join(
            "",
            name.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)
        );
        return sanitized.Trim();
    }

    public async Task ImportOrEnrichArtistAsync(
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
            logger.LogInformation("[ImportArtist] Creating new artist.json for '{Artist}' (MBID {MbId}) at {Path}", artistDisplayName, mbArtistId, artistJsonPath);
            var photos = await coverArtDownloadService.DownloadArtistPhotosAsync(
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
            logger.LogInformation("[ImportArtist] Enriching existing artist.json for '{Artist}' (MBID {MbId}) at {Path}", artistDisplayName, mbArtistId, artistJsonPath);
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
            // Always refresh artist photos during enrich to keep assets up to date
            try
            {
                var photos = await coverArtDownloadService.DownloadArtistPhotosAsync(
                    mbArtistId,
                    artistDir
                );
                jsonArtist.Photos = new JsonArtistPhotos
                {
                    Thumbs = photos.Thumbs.Any() ? photos.Thumbs : null,
                    Backgrounds = photos.Backgrounds.Any() ? photos.Backgrounds : null,
                    Banners = photos.Banners.Any() ? photos.Banners : null,
                    Logos = photos.Logos.Any() ? photos.Logos : null,
                };
            }
            catch { }
        }

        // Fetch enrichment (only if missing or we just created)
        try
        {
            // Always refresh aliases from MusicBrainz (lightweight and cached)
            try
            {
                var mbArtist = await musicBrainzService.GetArtistByIdAsync(mbArtistId);
                var aliases = mbArtist?.Aliases?.Select(a => new JsonArtistAlias
                {
                    Name = a.Name,
                    SortName = a.SortName,
                    BeginDate = a.Begin,
                    EndDate = a.End,
                    Type = a.Type,
                    Locale = a.Locale
                }).ToList();
                if (aliases != null && aliases.Any())
                {
                    jsonArtist.Aliases = aliases;
                }
                // Prefer MusicBrainz sort name if available
                if (!string.IsNullOrWhiteSpace(mbArtist?.SortName))
                {
                    jsonArtist.SortName = mbArtist!.SortName;
                }
            }
            catch { }

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

                // TOP TRACKS: Prefer Spotify, fallback to Last.fm
                try
                {
                    // Try to resolve Spotify artist id from existing connections, MusicBrainz relations, or search by name
                    jsonArtist.Connections ??= new JsonArtistServiceConnections();
                    string? spotifyArtistId = jsonArtist.Connections.SpotifyId;

                    if (string.IsNullOrWhiteSpace(spotifyArtistId))
                    {
                        try
                        {
                            var mbArtist = await musicBrainzService.GetArtistByIdAsync(mbArtistId);
                            var rels = mbArtist?.Relations;
                            if (rels != null)
                            {
                                var foundIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                                foreach (var rel in rels)
                                {
                                    var url = rel?.Url?.Resource;
                                    if (string.IsNullOrWhiteSpace(url)) continue;
                                    if (url.Contains("open.spotify.com/artist/", StringComparison.OrdinalIgnoreCase))
                                    {
                                        try
                                        {
                                            var id = new Uri(url).Segments.LastOrDefault()?.Trim('/');
                                            if (!string.IsNullOrWhiteSpace(id) && foundIds.Add(id))
                                            {
                                                jsonArtist.Connections.SpotifyIds ??= new List<JsonSpotifyArtistIdentity>();
                                                if (!jsonArtist.Connections.SpotifyIds.Any(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase)))
                                                {
                                                    jsonArtist.Connections.SpotifyIds.Add(new JsonSpotifyArtistIdentity
                                                    {
                                                        Id = id,
                                                        DisplayName = artistDisplayName,
                                                        Source = "musicbrainz",
                                                        AddedAt = DateTime.UtcNow.ToString("yyyy-MM-dd")
                                                    });
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                }
                                // Back-compat: set single SpotifyId to the first linked id if not already set
                                if (string.IsNullOrWhiteSpace(jsonArtist.Connections.SpotifyId))
                                {
                                    var first = jsonArtist.Connections.SpotifyIds?.FirstOrDefault()?.Id;
                                    if (!string.IsNullOrWhiteSpace(first))
                                    {
                                        spotifyArtistId = first;
                                        jsonArtist.Connections.SpotifyId = first;
                                    }
                                }
                            }
                        }
                        catch { }
                    }

                    if (string.IsNullOrWhiteSpace(spotifyArtistId))
                    {
                        try
                        {
                            var matches = await spotifyService.SearchArtistsAsync(artistDisplayName, 1);
                            var best = matches?.FirstOrDefault();
                            if (!string.IsNullOrWhiteSpace(best?.Id))
                            {
                                spotifyArtistId = best!.Id;
                                jsonArtist.Connections.SpotifyId = spotifyArtistId;
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
                            }
                        }
                        catch { }
                    }

                    // Fetch top tracks from all linked Spotify IDs, merged by play count
                    var allSpotifyIds = (jsonArtist.Connections.SpotifyIds?.Select(s => s.Id).ToList() ?? new List<string>());
                    if (!string.IsNullOrWhiteSpace(spotifyArtistId) && !allSpotifyIds.Contains(spotifyArtistId))
                    {
                        allSpotifyIds.Add(spotifyArtistId);
                    }

                    if (allSpotifyIds.Count > 0)
                    {
                        TopTracks.ITopTracksImporter spImporter = new TopTracks.TopTracksSpotifyImporter(spotifyService);
                        var merged = new Dictionary<string, JsonTopTrack>(StringComparer.OrdinalIgnoreCase);
                        foreach (var sid in allSpotifyIds)
                        {
                            try
                            {
                                var tracks = await spImporter.GetTopTracksAsync(sid, 25) ?? new List<JsonTopTrack>();
                                foreach (var t in tracks)
                                {
                                    var key = NormalizeTitle(t.Title);
                                    if (merged.TryGetValue(key, out var existing))
                                    {
                                        // keep highest play count
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
                            .Take(25)
                            .ToList();
                    }
                }
                catch { jsonArtist.TopTracks = jsonArtist.TopTracks ?? []; }

                // Fallback to Last.fm if Spotify not available or returned nothing
                if (jsonArtist.TopTracks == null || jsonArtist.TopTracks.Count == 0)
                {
                    try
                    {
                        TopTracks.ITopTracksImporter lfImporter = new TopTracks.TopTracksLastFmImporter(lastfmClient);
                        jsonArtist.TopTracks = await lfImporter.GetTopTracksAsync(mbArtistId, 25);
                    }
                    catch { jsonArtist.TopTracks = jsonArtist.TopTracks ?? []; }
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
                                        // Store path relative to artist folder so it resolves correctly from artist.json
                                        var combined = Path.Combine(folderName, relPath)
                                            .Replace('\\', '/');
                                        topTrack.CoverArt = "./" + combined;
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

                // Complete missing fields (releaseTitle/cover art) using Spotify data
                try
                {
                    var completer = new TopTracks.TopTracksCompleter(spotifyService, lastfmClient, logger);
                    await completer.CompleteAsync(artistDir, jsonArtist);
                }
                catch { }
            }
        }
        catch
        {
            // ignore enrichment failures
        }

        // Try to enrich external service connections using MusicBrainz relations and Spotify fallback
        try
        {
            jsonArtist.Connections ??= new JsonArtistServiceConnections();
            var mbArtist = await musicBrainzService.GetArtistByIdAsync(mbArtistId);
            var rels = mbArtist?.Relations;
            if (rels != null)
            {
                var spotifyIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var rel in rels)
                {
                    var url = rel?.Url?.Resource;
                    if (string.IsNullOrWhiteSpace(url)) continue;
                    if (url.Contains("open.spotify.com/artist/", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var id = new Uri(url).Segments.LastOrDefault()?.Trim('/');
                            if (!string.IsNullOrWhiteSpace(id))
                            {
                                jsonArtist.Connections.SpotifyId ??= id; // legacy single field
                                spotifyIds.Add(id);
                            }
                        }
                        catch { }
                    }
                    else if (url.Contains("music.apple.com", StringComparison.OrdinalIgnoreCase))
                    {
                        // Apple Music artist URLs often include /artist/<name>/<id>
                        var segments = new Uri(url).Segments.Select(s => s.Trim('/')).ToArray();
                        var maybeId = segments.LastOrDefault();
                        if (!string.IsNullOrWhiteSpace(maybeId) && maybeId.All(char.IsDigit))
                            jsonArtist.Connections.AppleMusicArtistId ??= maybeId;
                    }
                    else if (url.Contains("music.youtube.com", StringComparison.OrdinalIgnoreCase))
                    {
                        jsonArtist.Connections.YoutubeMusicUrl ??= url;
                    }
                    else if (url.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) || url.Contains("youtu.be", StringComparison.OrdinalIgnoreCase))
                    {
                        // Prefer channel URLs
                        if (url.Contains("/channel/") || url.Contains("/c/") || url.Contains("/@"))
                            jsonArtist.Connections.YoutubeChannelUrl ??= url;
                    }
                    else if (url.Contains("tidal.com", StringComparison.OrdinalIgnoreCase))
                    {
                        // Not always standardized; keep as id when possible
                        jsonArtist.Connections.TidalArtistId ??= url;
                    }
                    else if (url.Contains("deezer.com", StringComparison.OrdinalIgnoreCase))
                    {
                        jsonArtist.Connections.DeezerArtistId ??= url.Split('/').LastOrDefault();
                    }
                    else if (url.Contains("soundcloud.com", StringComparison.OrdinalIgnoreCase))
                    {
                        jsonArtist.Connections.SoundcloudUrl ??= url;
                    }
                    else if (url.Contains("bandcamp.com", StringComparison.OrdinalIgnoreCase))
                    {
                        jsonArtist.Connections.BandcampUrl ??= url;
                    }
                    else if (url.Contains("discogs.com", StringComparison.OrdinalIgnoreCase))
                    {
                        jsonArtist.Connections.DiscogsUrl ??= url;
                    }
                }

                if (spotifyIds.Count > 0)
                {
                    jsonArtist.Connections.SpotifyIds ??= new List<JsonSpotifyArtistIdentity>();
                    foreach (var sid in spotifyIds)
                    {
                        if (!jsonArtist.Connections.SpotifyIds.Any(x => string.Equals(x.Id, sid, StringComparison.OrdinalIgnoreCase)))
                        {
                            jsonArtist.Connections.SpotifyIds.Add(new JsonSpotifyArtistIdentity
                            {
                                Id = sid,
                                DisplayName = jsonArtist.Name,
                                Source = "musicbrainz",
                                AddedAt = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                            });
                        }
                    }
                }

                // Normalize any previously mis-assigned YouTube Music link
                if (!string.IsNullOrWhiteSpace(jsonArtist.Connections.YoutubeChannelUrl)
                    && jsonArtist.Connections.YoutubeChannelUrl.Contains("music.youtube.com", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(jsonArtist.Connections.YoutubeMusicUrl))
                    {
                        jsonArtist.Connections.YoutubeMusicUrl = jsonArtist.Connections.YoutubeChannelUrl;
                    }
                    jsonArtist.Connections.YoutubeChannelUrl = null;
                }
            }

            // If SpotifyId still missing, try to search by name as a fallback
            if (string.IsNullOrWhiteSpace(jsonArtist.Connections.SpotifyId))
            {
                try
                {
                    var spotifyMatches = await spotifyService.SearchArtistsAsync(artistDisplayName, 1);
                    var match = spotifyMatches?.FirstOrDefault();
                    if (match?.Id != null)
                    {
                        jsonArtist.Connections.SpotifyId = match.Id;
                    }
                }
                catch { }
            }
        }
        catch
        {
            // ignore enrich errors
        }

        await writer.WriteArtistAsync(jsonArtist);
        logger.LogInformation("[ImportArtist] Wrote artist.json for '{Artist}' at {Path}", jsonArtist.Name, artistJsonPath);
    }

    public async Task<int> ImportEligibleReleaseGroupsAsync(string artistDir, string mbArtistId)
    {
        logger.LogInformation("[ImportExecutor] 🚀 Starting import of eligible release groups for artist directory: {ArtistDir} (MBID: {MbId})", artistDir, mbArtistId);
        var startTime = DateTime.UtcNow;
        
        try
        {
            // Get all release groups for this artist
            logger.LogInformation("[ImportExecutor] 🔍 Fetching release groups from MusicBrainz for artist MBID: {MbId}", mbArtistId);
            var releaseGroupsStart = DateTime.UtcNow;
            var releaseGroups = await musicBrainzService.GetReleaseGroupsForArtistAsync(mbArtistId);
            var releaseGroupsDuration = DateTime.UtcNow - releaseGroupsStart;
            
            logger.LogInformation("[ImportExecutor] 📀 Found {ReleaseGroupCount} total release groups in {DurationMs}ms", releaseGroups.Count, releaseGroupsDuration.TotalMilliseconds);

            // Filter to only eligible types (Album, EP, Single)
            var eligibleTypes = new[] { "Album", "EP", "Single" };
            var eligibleGroups = releaseGroups.Where(rg => eligibleTypes.Contains(rg.PrimaryType)).ToList();
            
            // Filter to only include release groups where this artist is the primary credited artist
            var primaryArtistGroups = eligibleGroups.Where(rg =>
            {
                var credits = rg.Credits?.ToList();
                if (credits == null || credits.Count == 0) return false;
                
                // Check if the first (primary) credit is for this artist
                var primaryCredit = credits.FirstOrDefault();
                if (primaryCredit?.Artist?.Id == null) return false;
                
                // Basic primary artist check
                var isPrimaryArtist = primaryCredit.Artist.Id == mbArtistId;
                if (!isPrimaryArtist) return false;
                
                // Additional filtering to exclude non-official releases
                var title = rg.Title?.ToLowerInvariant() ?? "";
                var secondaryTypes = rg.SecondaryTypes?.Select(t => t.ToLowerInvariant()).ToList() ?? new List<string>();
                
                // Exclude compilations, anthologies, live recordings, mixtapes, etc.
                var excludeKeywords = new[]
                {
                    "anthology", "compilation", "collection", "greatest hits", "best of",
                    "live", "concert", "performance", "storytellers", "unplugged",
                    "mixtape", "presented by", "dj", "remix", "remastered",
                    "deluxe", "expanded", "special edition", "anniversary"
                };
                
                // Check if title contains any exclude keywords
                if (excludeKeywords.Any(keyword => title.Contains(keyword)))
                {
                    return false;
                }
                
                // Specific exclusions for albums that should not be imported
                var specificExcludeTitles = new[]
                {
                    "the college dropout video anthology",
                    "late orchestration",
                    "can't tell me nothing: the official mixtape",
                    "sky high: presented by dj benzi and plain pat",
                    "good fridays",
                    "vh1 storytellers"
                };
                
                if (specificExcludeTitles.Any(excludeTitle => title.Contains(excludeTitle)))
                {
                    return false;
                }
                
                // Check secondary types that indicate non-studio releases
                var excludeSecondaryTypes = new[]
                {
                    "compilation", "live", "mixtape", "remix", "dj-mix"
                };
                
                if (excludeSecondaryTypes.Any(excludeType => secondaryTypes.Contains(excludeType)))
                {
                    return false;
                }
                
                return true;
            }).ToList();
            
            // Special handling for important collaborations that should be included
            var importantCollaborations = eligibleGroups.Where(rg =>
            {
                var credits = rg.Credits?.ToList();
                if (credits == null || credits.Count == 0) return false;
                
                // Check if this artist appears but is not the primary artist
                var hasArtist = credits.Any(c => c.Artist?.Id == mbArtistId);
                if (!hasArtist) return false;
                
                var primaryCredit = credits.FirstOrDefault();
                if (primaryCredit?.Artist?.Id == null) return false;
                
                // Only include if this artist is not the primary artist
                if (primaryCredit.Artist.Id == mbArtistId) return false;
                
                // Check for specific important collaborations
                var title = rg.Title?.ToLowerInvariant() ?? "";
                var importantCollaborationTitles = new[]
                {
                    "watch the throne", // Kanye West + Jay-Z collaboration
                    "kids see ghosts", // Kanye West + Kid Cudi collaboration
                    "cruel summer", // GOOD Music compilation (but important)
                    "ye vs. the people" // Kanye West + T.I. collaboration
                };
                
                return importantCollaborationTitles.Any(importantTitle => title.Contains(importantTitle));
            }).ToList();
            
            // Combine primary artist groups with important collaborations
            var finalImportGroups = primaryArtistGroups.Concat(importantCollaborations).ToList();
            
            // Collect non-primary artist appearances for the alsoAppearsOn field
            var nonPrimaryArtistGroups = eligibleGroups.Where(rg =>
            {
                var credits = rg.Credits?.ToList();
                if (credits == null || credits.Count == 0) return false;
                
                // Check if this artist appears but is not the primary artist
                var hasArtist = credits.Any(c => c.Artist?.Id == mbArtistId);
                if (!hasArtist) return false;
                
                var primaryCredit = credits.FirstOrDefault();
                if (primaryCredit?.Artist?.Id == null) return false;
                
                // Exclude important collaborations that we're already importing
                var title = rg.Title?.ToLowerInvariant() ?? "";
                var importantCollaborationTitles = new[]
                {
                    "watch the throne", // Kanye West + Jay-Z collaboration
                    "kids see ghosts", // Kanye West + Kid Cudi collaboration
                    "cruel summer", // GOOD Music compilation (but important)
                    "ye vs. the people" // Kanye West + T.I. collaboration
                };
                
                if (importantCollaborationTitles.Any(importantTitle => title.Contains(importantTitle)))
                {
                    return false;
                }
                
                return primaryCredit.Artist.Id != mbArtistId;
            }).ToList();
            
            logger.LogInformation("[ImportExecutor] ✅ Filtered to {EligibleCount}/{TotalCount} eligible release groups (types: {Types})", 
                eligibleGroups.Count, releaseGroups.Count, string.Join(", ", eligibleTypes));
            logger.LogInformation("[ImportExecutor] 🎯 Further filtered to {PrimaryArtistCount}/{EligibleCount} primary artist release groups", 
                primaryArtistGroups.Count, eligibleGroups.Count);
            logger.LogInformation("[ImportExecutor] 🤝 Found {ImportantCollaborationCount} important collaborations to include", 
                importantCollaborations.Count);
            logger.LogInformation("[ImportExecutor] 📥 Final import count: {FinalCount} release groups", 
                finalImportGroups.Count);
            logger.LogInformation("[ImportExecutor] 🤝 Found {NonPrimaryCount} non-primary artist appearances for alsoAppearsOn", 
                nonPrimaryArtistGroups.Count);

            var importedCount = 0;
            var skippedCount = 0;
            var failedCount = 0;
            var importStart = DateTime.UtcNow;

            foreach (var releaseGroup in finalImportGroups)
            {
                try
                {
                    logger.LogInformation("[ImportExecutor] 📀 Processing release group: '{Title}' (Type: {PrimaryType}, ID: {ReleaseGroupId})", 
                        releaseGroup.Title, releaseGroup.PrimaryType, releaseGroup.Id);
                    
                    var singleGroupStart = DateTime.UtcNow;
                    
                    // Check if this release group already exists
                    var releaseDir = SanitizeFolderName(releaseGroup.Title);
                    var releasePath = Path.Combine(artistDir, releaseDir);
                    
                    if (Directory.Exists(releasePath))
                    {
                        logger.LogInformation("[ImportExecutor] ℹ️ Release directory already exists, skipping: {ReleasePath}", releasePath);
                        skippedCount++;
                        continue;
                    }

                    logger.LogInformation("[ImportExecutor] 🆕 Creating new release directory: {ReleasePath}", releasePath);
                    Directory.CreateDirectory(releasePath);

                    // Import the release group
                    logger.LogInformation("[ImportExecutor] 📥 Importing release group data and cover art");
                    await ImportReleaseIfMissingAsync(
                        artistDir,
                        releasePath,
                        releaseGroup.Id,
                        releaseGroup.Title,
                        releaseGroup.PrimaryType
                    );
                    
                    var singleGroupDuration = DateTime.UtcNow - singleGroupStart;
                    
                    // Check if the import was successful by looking for release.json
                    var releaseJsonPath = Path.Combine(releasePath, "release.json");
                    if (File.Exists(releaseJsonPath))
                    {
                        importedCount++;
                        logger.LogInformation("[ImportExecutor] ✅ Successfully imported release group '{Title}' in {DurationMs}ms", 
                            releaseGroup.Title, singleGroupDuration.TotalMilliseconds);
                    }
                    else
                    {
                        failedCount++;
                        logger.LogWarning("[ImportExecutor] ⚠️ Failed to import release group '{Title}' after {DurationMs}ms - no release.json created", 
                            releaseGroup.Title, singleGroupDuration.TotalMilliseconds);
                    }
                }
                catch (Exception ex)
                {
                    failedCount++;
                    logger.LogError(ex, "[ImportExecutor] ❌ Exception while processing release group '{Title}'", releaseGroup.Title);
                }
            }

            var totalImportDuration = DateTime.UtcNow - importStart;
            var totalDuration = DateTime.UtcNow - startTime;
            
            // Populate alsoAppearsOn field in artist.json with non-primary artist appearances
            if (nonPrimaryArtistGroups != null && nonPrimaryArtistGroups.Count > 0)
            {
                try
                {
                    var artistJsonPath = Path.Combine(artistDir, "artist.json");
                    if (File.Exists(artistJsonPath))
                    {
                        var artistText = await File.ReadAllTextAsync(artistJsonPath);
                        var jsonArtist = JsonSerializer.Deserialize<JsonArtist>(artistText, GetJsonOptions()) ?? new JsonArtist();
                        
                        logger.LogInformation("[ImportExecutor] 🖼️ Downloading cover art for {Count} non-primary artist appearances", nonPrimaryArtistGroups.Count);
                        
                        var appearancesWithCoverArt = new List<JsonArtistAppearance>();
                        
                        foreach (var rg in nonPrimaryArtistGroups)
                        {
                            try
                            {
                                var credits = rg.Credits?.ToList();
                                var primaryCredit = credits?.FirstOrDefault();
                                var primaryArtistName = primaryCredit?.Artist?.Name ?? primaryCredit?.Name ?? "Unknown Artist";
                                var primaryArtistId = primaryCredit?.Artist?.Id;
                                
                                // Determine the role of this artist on the release
                                var artistCredit = credits?.FirstOrDefault(c => c.Artist?.Id == mbArtistId);
                                var role = artistCredit?.JoinPhrase?.TrimStart(' ', '&', ',') ?? "Featured Artist";
                                
                                // Fetch cover art for this release group
                                string? coverArtUrl = null;
                                try
                                {
                                    // Try to get cover art URL from Cover Art Archive and download it locally
                                    var releases = await musicBrainzService.GetReleasesForReleaseGroupAsync(rg.Id);
                                    var bestRelease = releases.FirstOrDefault();
                                    if (bestRelease?.Id != null)
                                    {
                                        // Try to download cover art from Cover Art Archive
                                        try
                                        {
                                            using var httpClient = new HttpClient();
                                            var coverArtSourceUrl = $"https://coverartarchive.org/release/{bestRelease.Id}/front-500.jpg";
                                            
                                            var response = await httpClient.GetAsync(coverArtSourceUrl);
                                            if (response.IsSuccessStatusCode)
                                            {
                                                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                                                var fileName = $"appearance_{rg.Id}_cover.jpg";
                                                var coverArtPath = Path.Combine(artistDir, fileName);
                                                
                                                await File.WriteAllBytesAsync(coverArtPath, imageBytes);
                                                coverArtUrl = $"./{fileName}";
                                                
                                                logger.LogDebug("[ImportExecutor] 🖼️ Downloaded cover art for '{Title}': {CoverArtPath}", rg.Title, coverArtPath);
                                            }
                                            else
                                            {
                                                // Try alternative URL without size specification
                                                coverArtSourceUrl = $"https://coverartarchive.org/release/{bestRelease.Id}/front";
                                                response = await httpClient.GetAsync(coverArtSourceUrl);
                                                if (response.IsSuccessStatusCode)
                                                {
                                                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                                                    var fileName = $"appearance_{rg.Id}_cover.jpg";
                                                    var coverArtPath = Path.Combine(artistDir, fileName);
                                                    
                                                    await File.WriteAllBytesAsync(coverArtPath, imageBytes);
                                                    coverArtUrl = $"./{fileName}";
                                                    
                                                    logger.LogDebug("[ImportExecutor] 🖼️ Downloaded cover art for '{Title}' (alternative URL): {CoverArtPath}", rg.Title, coverArtPath);
                                                }
                                            }
                                        }
                                        catch (Exception downloadEx)
                                        {
                                            logger.LogDebug(downloadEx, "[ImportExecutor] ⚠️ Failed to download cover art for '{Title}' from Cover Art Archive", rg.Title);
                                        }
                                    }
                                }
                                catch (Exception coverArtEx)
                                {
                                    logger.LogDebug(coverArtEx, "[ImportExecutor] ⚠️ Failed to fetch cover art for release group '{Title}' (ID: {ReleaseGroupId})", rg.Title, rg.Id);
                                }
                                
                                var appearance = new JsonArtistAppearance
                                {
                                    ReleaseTitle = rg.Title ?? "Unknown Release",
                                    ReleaseType = rg.PrimaryType ?? "Unknown",
                                    PrimaryArtistName = primaryArtistName,
                                    PrimaryArtistMusicBrainzId = primaryArtistId,
                                    MusicBrainzReleaseGroupId = rg.Id,
                                    FirstReleaseDate = rg.FirstReleaseDate,
                                    FirstReleaseYear = rg.FirstReleaseDate?.Split("-").FirstOrDefault(),
                                    Role = role,
                                    CoverArt = coverArtUrl
                                };
                                
                                appearancesWithCoverArt.Add(appearance);
                            }
                            catch (Exception appearanceEx)
                            {
                                logger.LogWarning(appearanceEx, "[ImportExecutor] ⚠️ Failed to process appearance for release group '{Title}'", rg.Title);
                            }
                        }
                        
                        jsonArtist.AlsoAppearsOn = appearancesWithCoverArt;
                        
                        // Write updated artist.json
                        var updatedArtistText = JsonSerializer.Serialize(jsonArtist, GetJsonOptions());
                        await File.WriteAllTextAsync(artistJsonPath, updatedArtistText);
                        
                        var coverArtCount = appearancesWithCoverArt.Count(a => !string.IsNullOrEmpty(a.CoverArt));
                        logger.LogInformation("[ImportExecutor] 📝 Added {AppearanceCount} appearances to alsoAppearsOn field in artist.json ({CoverArtCount} with locally stored cover art)", 
                            appearancesWithCoverArt.Count, coverArtCount);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "[ImportExecutor] ⚠️ Failed to update alsoAppearsOn field in artist.json");
                }
            }
            
            logger.LogInformation("[ImportExecutor] 🎉 Release group import completed in {TotalDurationMs}ms", totalDuration.TotalMilliseconds);
            logger.LogInformation("[ImportExecutor] 📊 Import Summary: {Imported} imported, {Skipped} skipped, {Failed} failed", 
                importedCount, skippedCount, failedCount);
            logger.LogInformation("[ImportExecutor] ⏱️ Timing: Release groups fetch: {GroupsMs}ms, Individual imports: {ImportsMs}ms", 
                releaseGroupsDuration.TotalMilliseconds, totalImportDuration.TotalMilliseconds);

            return importedCount;
        }
        catch (Exception ex)
        {
            var totalDuration = DateTime.UtcNow - startTime;
            logger.LogError(ex, "[ImportExecutor] ❌ Failed to import eligible release groups for artist directory '{ArtistDir}' after {TotalDurationMs}ms", 
                artistDir, totalDuration.TotalMilliseconds);
            throw;
        }
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

        var folderName = Path.GetFileName(releaseDir) ?? string.Empty;
        var built = await releaseJsonBuilder.BuildAsync(
            artistDir,
            releaseGroupId,
            folderName,
            releaseTitle,
            primaryType
        );
        if (built is null)
        {
            logger.LogWarning(
                "[ImportRelease] No suitable release selected for group {ReleaseGroupId}",
                releaseGroupId
            );
            return;
        }

        await writer.WriteReleaseAsync(
            Path.GetFileName(artistDir) ?? string.Empty,
            folderName,
            built
        );
        await EnsureAudioFilePathsAsync(releaseDir, releaseJsonPath);
        logger.LogDebug("[ImportRelease] Wrote release.json for '{Title}' at {Path}", built.Title, releaseJsonPath);
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
