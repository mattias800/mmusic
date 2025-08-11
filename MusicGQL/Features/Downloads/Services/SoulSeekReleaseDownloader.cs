using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;
using HotChocolate.Subscriptions;
using MusicGQL.Features.Downloads.Util;
using MusicGQL.Features.External.SoulSeek.Integration;
using MusicGQL.Features.ServerLibrary.Cache;
using Soulseek;
using Directory = System.IO.Directory;
using Path = System.IO.Path;

namespace MusicGQL.Features.Downloads.Services;

public class SoulSeekReleaseDownloader(
    SoulSeekService service,
    ISoulseekClient client,
    ITopicEventSender eventSender,
    ServerLibraryCache cache,
    CurrentDownloadStateService progress,
    DownloadQueueService downloadQueue,
    ILogger<SoulSeekReleaseDownloader> logger
)
{
    public async Task<bool> DownloadReleaseAsync(
        string artistId,
        string releaseFolderName,
        string artistName,
        string releaseTitle,
        string targetDirectory
    )
    {
        if (service.State.NetworkState != SoulSeekNetworkState.Online)
        {
            await service.Connect();
        }

        Directory.CreateDirectory(targetDirectory);
        logger.LogInformation(
            "[SoulSeek] Searching: {Artist} - {Release}",
            artistName,
            releaseTitle
        );

        // Normalize base query (e.g., remove punctuation) to improve matching
        string normArtist = NormalizeForSearch(artistName);
        string normTitle = NormalizeForSearch(releaseTitle);
        var baseQuery = $"{normArtist} - {normTitle}".Trim();
        logger.LogInformation("[SoulSeek] Normalized search query: {Query}", baseQuery);

        // Determine expected track count from cache (if available)
        var cachedRelease = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);
        int expectedTrackCount = cachedRelease?.Tracks?.Count ?? 0;
        progress.Set(new DownloadProgress
        {
            ArtistId = artistId,
            ReleaseFolderName = releaseFolderName,
            Status = DownloadStatus.Searching,
            TotalTracks = expectedTrackCount,
            CompletedTracks = 0
        });
        int minRequiredTracks = expectedTrackCount > 0
            ? Math.Max(2, Math.Min(expectedTrackCount, expectedTrackCount - 1))
            : 5; // allow off-by-one if known, else require at least 5

        // Discover (optional) year for ranking (we already have expectedTrackCount above)
        var expectedYear = cachedRelease?.JsonRelease?.FirstReleaseYear;

        // Try multiple query forms: base "Artist - Title", folder-style separators, and optionally with year
        var queries = new List<string> { baseQuery };
        queries.Add($"{normArtist}\\{normTitle}");
        queries.Add($"{normArtist}/{normTitle}");

        // Accent-insensitive alternatives (e.g., Skeletá -> Skeleta)
        string foldArtist = RemoveDiacritics(normArtist);
        string foldTitle = RemoveDiacritics(normTitle);
        var baseQueryFold = $"{foldArtist} - {foldTitle}".Trim();
        if (!string.Equals(baseQueryFold, baseQuery, StringComparison.Ordinal))
        {
            queries.Add(baseQueryFold);
            queries.Add($"{foldArtist}\\{foldTitle}");
            queries.Add($"{foldArtist}/{foldTitle}");
        }
        if (!string.IsNullOrWhiteSpace(expectedYear))
        {
            queries.Add($"{normArtist} - {normTitle} ({expectedYear})");
            queries.Add($"{normArtist}\\{normTitle} ({expectedYear})");
            queries.Add($"{normArtist}/{normTitle} ({expectedYear})");

            if (!string.Equals(baseQueryFold, baseQuery, StringComparison.Ordinal))
            {
                queries.Add($"{foldArtist} - {foldTitle} ({expectedYear})");
                queries.Add($"{foldArtist}\\{foldTitle} ({expectedYear})");
                queries.Add($"{foldArtist}/{foldTitle} ({expectedYear})");
            }
        }

        List<SearchResponse> rankedCandidates = new();
        foreach (var q in queries.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var result = await client.SearchAsync(new SearchQuery(q));
            // First, require folder structure that matches Artist/<Album> patterns sufficiently
            var structurallyMatching = result.Responses
                .Where(r => HasAlbumFolderMatch(r, artistName, releaseTitle, expectedYear))
                .ToList();

            var ranked = structurallyMatching
                .Select(r => new
                {
                    Response = r,
                    Audio320Files = r.Files.Where(f =>
                        f.Extension.Equals("mp3", StringComparison.OrdinalIgnoreCase) && f.BitRate == 320).ToList(),
                    Score = ComputeCandidateScore(r, artistName, releaseTitle, expectedYear)
                })
                .Where(x => x.Audio320Files.Count >= (expectedTrackCount > 0
                    ? Math.Max(2, Math.Min(expectedTrackCount, expectedTrackCount - 1))
                    : 5))
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Response.QueueLength)
                .ThenBy(x => x.Response.HasFreeUploadSlot)
                .ThenByDescending(x => x.Response.UploadSpeed)
                .ThenByDescending(x => x.Audio320Files.Count)
                .Select(x => x.Response)
                .ToList();

            if (ranked.Count > 0)
            {
                rankedCandidates = ranked;
                break;
            }
        }

        // Use ranked candidates from the first query that returned any matches
        var orderedCandidates = rankedCandidates;

        if (orderedCandidates.Count == 0)
        {
            logger.LogWarning(
                "[SoulSeek] No suitable album candidates for: {Artist} - {Release}. Try adjusting search or availability.",
                artistName,
                releaseTitle
            );
            return false;
        }

        // Transition to Downloading state once we have a candidate
        await cache.UpdateReleaseDownloadStatus(
            artistId,
            releaseFolderName,
            CachedReleaseDownloadStatus.Downloading
        );
        progress.SetStatus(DownloadStatus.Downloading);

        foreach (var candidate in orderedCandidates)
        {
            bool userFailed = false;
            var queue = DownloadQueueFactory.Create(candidate);
            int trackIndex = 0;

            logger.LogInformation(
                "[SoulSeek] Trying user '{User}' with {Count} files",
                candidate.Username,
                queue.Count
            );

            if (queue.Count < minRequiredTracks)
            {
                // Defensive: skip users that don't have enough audio tracks even after queue creation filtering
                logger.LogInformation(
                    "[SoulSeek] Skipping user '{User}' due to insufficient tracks: {Count} < {Min}",
                    candidate.Username,
                    queue.Count,
                    minRequiredTracks
                );
                continue;
            }

            while (queue.Any())
            {
                var item = queue.Dequeue();
                var localPath = Path.Combine(targetDirectory, item.LocalFileName);
                var localDir = Path.GetDirectoryName(localPath);
                if (!string.IsNullOrWhiteSpace(localDir))
                    Directory.CreateDirectory(localDir);
                logger.LogInformation(
                    "[SoulSeek] Downloading {File} to {Path}",
                    item.FileName,
                    localPath
                );

                await cache.UpdateMediaAvailabilityStatus(
                    artistId,
                    releaseFolderName,
                    trackIndex + 1,
                    CachedMediaAvailabilityStatus.Downloading
                );

                try
                {
                    await client.DownloadAsync(item.Username, item.FileName, localPath);
                }
                catch (Exception ex)
                {
                    // If download fails for this user, skip the rest of this user's files
                    logger.LogWarning(
                        ex,
                        "[SoulSeek] Download failed from user '{User}' for file {File}. Skipping user and trying next candidate...",
                        item.Username,
                        item.FileName
                    );
                    userFailed = true;
                }

                if (userFailed)
                {
                    break;
                }

                await cache.UpdateMediaAvailabilityStatus(
                    artistId,
                    releaseFolderName,
                    trackIndex + 1,
                    CachedMediaAvailabilityStatus.Processing
                );

                trackIndex++;
                try
                {
                    progress.SetTrackProgress(trackIndex, expectedTrackCount);
                }
                catch
                {
                }
            }

            if (!userFailed)
            {
                logger.LogInformation(
                    "[SoulSeek] Download complete: {Artist} - {Release} (user {User})",
                    artistName,
                    releaseTitle,
                    candidate.Username
                );
                progress.SetStatus(DownloadStatus.Completed);
                if (!downloadQueue.TryDequeue(out var nxt) || nxt is null)
                {
                    progress.Reset();
                }
                else
                {
                    downloadQueue.Enqueue(nxt);
                }

                return true;
            }

            // try next candidate
            logger.LogInformation(
                "[SoulSeek] Moving to next candidate user after failure: {User}",
                candidate.Username
            );
        }

        logger.LogWarning(
            "[SoulSeek] All candidates failed for: {Artist} - {Release}",
            artistName,
            releaseTitle
        );
        progress.SetStatus(DownloadStatus.Failed);
        return false;
    }

    private static string NormalizeForSearch(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        // Keep only letters, numbers, and spaces; collapse multiple spaces
        // \p{L} = any kind of letter from any language; \p{N} = any kind of numeric character
        var alnumSpaceOnly = Regex.Replace(input, @"[^\p{L}\p{N}\s]", " ");
        return Regex.Replace(alnumSpaceOnly, @"\s+", " ").Trim();
    }

    private static int ComputeCandidateScore(SearchResponse response, string artistName, string releaseTitle,
        string? expectedYear)
    {
        // Heuristic: prefer responses whose file paths look like .../<Artist>/<Album (Year)>/Track
        // Also reward when album segment matches release title and appears after artist segment
        int score = 0;
        if (response.Files == null || response.Files.Count == 0) return score;

        string nArtist = NormalizeForCompare(artistName);
        string nTitle = NormalizeForCompare(releaseTitle);
        string? nYear = string.IsNullOrWhiteSpace(expectedYear) ? null : expectedYear;

        int considered = 0;
        int strongMatches = 0;
        var albumFolderKeyCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var f in response.Files)
        {
            if (!f.Extension.Equals("mp3", StringComparison.OrdinalIgnoreCase)) continue;
            considered++;

            var normalizedPath = f.Filename.Replace('\\', '/');
            var segments = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            // Normalize segments for compare
            var normSegs = segments.Select(NormalizeForCompare).ToList();

            int idxArtist = normSegs.FindIndex(s => s.Equals(nArtist, StringComparison.OrdinalIgnoreCase));
            // Album segment candidates: after artist index, pick the next segment that contains title
            int idxAlbum = -1;
            if (idxArtist >= 0)
            {
                for (int i = idxArtist + 1; i < normSegs.Count; i++)
                {
                    if (normSegs[i].Contains(nTitle, StringComparison.OrdinalIgnoreCase) ||
                        nTitle.Contains(normSegs[i], StringComparison.OrdinalIgnoreCase))
                    {
                        idxAlbum = i;
                        break;
                    }
                }
            }

            bool pathOrderOk = idxArtist >= 0 && idxAlbum > idxArtist;
            if (pathOrderOk) score += 10;

            if (idxAlbum >= 0)
            {
                // Reward close/equal match for album folder name
                var albumSeg = normSegs[idxAlbum];
                if (albumSeg.Equals(nTitle, StringComparison.OrdinalIgnoreCase)) score += 20;
                if (albumSeg.Contains(nTitle, StringComparison.OrdinalIgnoreCase)) score += 10;

                // Reward matching year inside the album segment
                if (!string.IsNullOrWhiteSpace(nYear) &&
                    segments[idxAlbum].Contains(nYear!, StringComparison.OrdinalIgnoreCase))
                {
                    score += 10;
                }

                // Track dominant album folder grouping
                // Use the original-cased pair of artist+album segments for a grouping key when available
                string key = idxArtist >= 0 ? $"{segments[idxArtist]}///{segments[idxAlbum]}" : segments[idxAlbum];
                albumFolderKeyCounts[key] = albumFolderKeyCounts.GetValueOrDefault(key) + 1;
                strongMatches++;
            }
        }

        if (considered > 0)
        {
            // Scale based on fraction of strong matches
            score += (int)(100 * (double)strongMatches / considered);

            // Reward cohesion: many files in the same album folder key
            if (albumFolderKeyCounts.Count > 0)
            {
                int maxGroup = albumFolderKeyCounts.Values.Max();
                score += Math.Min(100, maxGroup * 2);
            }
        }

        return score;
    }

    private static bool HasAlbumFolderMatch(SearchResponse response, string artistName, string releaseTitle,
        string? expectedYear)
    {
        if (response.Files == null || response.Files.Count == 0) return false;
        string nArtist = NormalizeForCompare(artistName);
        string nTitle = NormalizeForCompare(releaseTitle);
        string? nYear = string.IsNullOrWhiteSpace(expectedYear) ? null : expectedYear;

        int matches = 0;
        foreach (var f in response.Files)
        {
            if (!f.Extension.Equals("mp3", StringComparison.OrdinalIgnoreCase)) continue;
            var normalizedPath = f.Filename.Replace('\\', '/');
            var segments = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var normSegs = segments.Select(NormalizeForCompare).ToList();
            int idxArtist = normSegs.FindIndex(s => s.Equals(nArtist, StringComparison.OrdinalIgnoreCase));
            if (idxArtist < 0) continue;
            int idxAlbum = -1;
            for (int i = idxArtist + 1; i < normSegs.Count; i++)
            {
                if (normSegs[i].Contains(nTitle, StringComparison.OrdinalIgnoreCase) ||
                    nTitle.Contains(normSegs[i], StringComparison.OrdinalIgnoreCase))
                {
                    idxAlbum = i;
                    break;
                }
            }

            if (idxAlbum > idxArtist)
            {
                // Optional: reinforce with year presence when known
                if (!string.IsNullOrWhiteSpace(nYear))
                {
                    if (segments[idxAlbum].Contains(nYear!, StringComparison.OrdinalIgnoreCase))
                        matches++;
                    else
                        matches += 0; // still count album match without year
                }
                else
                {
                    matches++;
                }
            }
        }

        // Require at least a few files to match the folder pattern to avoid wrong albums
        return matches >= 3; // threshold
    }

    private static string NormalizeForCompare(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        // Lowercase and remove diacritics to compare accent-insensitively
        var lowered = input.ToLowerInvariant();
        var folded = RemoveDiacritics(lowered);
        var filtered = Regex.Replace(folded, @"[^\p{L}\p{N}\s]", " ");
        return Regex.Replace(filtered, @"\s+", " ").Trim();
    }

    private static string RemoveDiacritics(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var normalized = input.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(input.Length);
        for (int i = 0; i < normalized.Length; i++)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(normalized[i]);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(normalized[i]);
            }
        }
        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}