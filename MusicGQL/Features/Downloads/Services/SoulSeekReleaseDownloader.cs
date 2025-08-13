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
    DownloadHistoryService downloadHistory,
    ServerSettings.ServerSettingsAccessor settingsAccessor,
    ILogger<SoulSeekReleaseDownloader> logger
)
{
    public async Task<bool> DownloadReleaseAsync(
        string artistId,
        string releaseFolderName,
        string artistName,
        string releaseTitle,
        string targetDirectory,
        List<int> allowedOfficialCounts,
        List<int> allowedOfficialDigitalCounts,
        CancellationToken cancellationToken = default
    )
    {
        if (service.State.NetworkState != SoulSeekNetworkState.Online)
        {
            logger.LogInformation("[SoulSeek] Client not online. Connecting...");
            await service.Connect();
            logger.LogInformation("[SoulSeek] Client network state after connect: {State}", service.State.NetworkState);
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
            CompletedTracks = 0,
            ArtistName = cachedRelease?.ArtistName ?? artistName,
            ReleaseTitle = cachedRelease?.Title ?? releaseTitle,
            CoverArtUrl = Features.ServerLibrary.Utils.LibraryAssetUrlFactory.CreateReleaseCoverArtUrl(artistId, releaseFolderName)
        });
        int minRequiredTracks = expectedTrackCount > 0
            ? expectedTrackCount
            : 5; // if unknown, require at least 5 to avoid noise
        // When we have authoritative allowed counts (official/digital), bypass generic min requirement
        bool useStrictAllowedCounts = (allowedOfficialDigitalCounts is { Count: > 0 }) || (allowedOfficialCounts is { Count: > 0 });
        if (useStrictAllowedCounts)
        {
            minRequiredTracks = 0;
        }
        logger.LogInformation("[SoulSeek] Expected tracks={Expected}, minRequiredTracks={Min} (strictAllowedCounts={Strict})", expectedTrackCount, minRequiredTracks, useStrictAllowedCounts);
        if (allowedOfficialCounts.Count > 0)
        {
            try { logger.LogInformation("[SoulSeek] Allowed official track counts: {Counts}", string.Join(", ", allowedOfficialCounts)); } catch { }
        }
        if (allowedOfficialDigitalCounts.Count > 0)
        {
            try { logger.LogInformation("[SoulSeek] Allowed official DIGITAL track counts: {Counts}", string.Join(", ", allowedOfficialDigitalCounts)); } catch { }
        }

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
        // Time-slicing: if queue has waiting jobs, enforce a time limit for search to avoid blocking
        int queueDepth = downloadQueue.Snapshot().QueueLength;
        bool hasWaiting = queueDepth > 0;
        // Read setting via cache of server settings through DI (access via separate resolver)
        int timeLimitSec = 60;
        int noDataTimeoutSec = 20;
        try { var s = await settingsAccessor.GetAsync(); timeLimitSec = Math.Max(5, s.SoulSeekSearchTimeLimitSeconds); noDataTimeoutSec = Math.Max(5, s.SoulSeekNoDataTimeoutSeconds); } catch { }
        // Shared time budget across all query forms when queue has waiting items
        TimeSpan totalSearchBudget = TimeSpan.FromSeconds(timeLimitSec);
        var budgetStartUtc = DateTime.UtcNow;
        var distinctQueries = queries.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        logger.LogInformation("[SoulSeek] Starting search across {QueryForms} query forms (queueDepth={QueueDepth}, sharedTimeBudgetSec={Budget})", distinctQueries.Count, queueDepth, timeLimitSec);
        for (int qi = 0; qi < distinctQueries.Count; qi++)
        {
            var q = distinctQueries[qi];
            var searchTask = client.SearchAsync(new SearchQuery(q));
            try
            {
                if (hasWaiting)
                {
                    var elapsed = DateTime.UtcNow - budgetStartUtc;
                    var remaining = totalSearchBudget - elapsed;
                    if (remaining <= TimeSpan.Zero)
                    {
                        logger.LogInformation("[SoulSeek] Search budget exhausted before query: {Query}", q);
                        break;
                    }
                    // Evenly divide remaining budget across remaining query forms
                    int remainingQueries = distinctQueries.Count - qi;
                    var slice = TimeSpan.FromMilliseconds(Math.Max(5, remaining.TotalMilliseconds / Math.Max(1, remainingQueries)));
                    var completed = await Task.WhenAny(searchTask, Task.Delay(slice));
                    if (completed != searchTask)
                    {
                        logger.LogInformation("[SoulSeek] Query slice timed out (moving to next form): {Query} (sliceMs={SliceMs:n0})", q, slice.TotalMilliseconds);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[SoulSeek] Search error on query {Query}", q);
                continue;
            }
            var result = await searchTask;
            logger.LogDebug("[SoulSeek] Search completed for '{Query}'. Responses={Count}", q, result.Responses?.Count ?? 0);
            // First, require folder structure that matches Artist/<Album> patterns sufficiently
            var structurallyMatching = result.Responses
                .Where(r => HasAlbumFolderMatch(r, artistName, releaseTitle, expectedYear))
                .ToList();
            logger.LogDebug("[SoulSeek] Structurally matching responses for '{Query}': {Count}", q, structurallyMatching.Count);

            var ranked = structurallyMatching
                .Select(r => new
                {
                    Response = r,
                    Audio320Files = r.Files.Where(f =>
                        f.Extension.Equals("mp3", StringComparison.OrdinalIgnoreCase) && f.BitRate == 320).ToList(),
                    Score = ComputeCandidateScore(
                        r,
                        artistName,
                        releaseTitle,
                        expectedYear,
                        expectedTrackCount,
                        allowedOfficialCounts,
                        allowedOfficialDigitalCounts
                    )
                })
                .Where(x =>
                {
                    // If we have official allowed counts, require exact match to one of them
                    if (allowedOfficialDigitalCounts is { Count: > 0 })
                    {
                        // Prefer digital: require match to digital counts if available
                        return allowedOfficialDigitalCounts.Contains(x.Audio320Files.Count);
                    }
                    if (allowedOfficialCounts is { Count: > 0 })
                    {
                        return allowedOfficialCounts.Contains(x.Audio320Files.Count);
                    }
                    // Otherwise, require exact match to expected track count when known
                    if (expectedTrackCount > 0)
                    {
                        return x.Audio320Files.Count == expectedTrackCount;
                    }
                    // As a last resort when nothing is known, require at least 5 tracks
                    return x.Audio320Files.Count >= 5;
                })
                // Under load, skip responses that have no free slot and long queues
                .Where(x => !hasWaiting || x.Response.HasFreeUploadSlot || x.Response.QueueLength <= 3)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Response.HasFreeUploadSlot)
                .ThenBy(x => x.Response.QueueLength)
                .ThenByDescending(x => x.Response.UploadSpeed)
                .ThenByDescending(x => x.Audio320Files.Count)
                .Select(x => x.Response)
                .ToList();
            logger.LogDebug("[SoulSeek] Ranked candidates for '{Query}': {Count}", q, ranked.Count);

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
            // Record not found in history
            try
            {
                downloadHistory.Add(new DownloadHistoryItem(
                    DateTime.UtcNow,
                    artistId,
                    releaseFolderName,
                    artistName,
                    releaseTitle,
                    false,
                    "Not found"
                ));
            }
            catch { }
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
            var queue = DownloadQueueFactory.Create(
                candidate,
                artistName,
                releaseTitle,
                expectedTrackCount,
                cachedRelease?.Tracks?.Where(t => !string.IsNullOrWhiteSpace(t.Title)).Select(t => t.Title!)
            );
            int trackIndex = 0;

            logger.LogInformation(
                "[SoulSeek] Trying user '{User}' with {Count} files",
                candidate.Username,
                queue.Count
            );
            logger.LogDebug("[SoulSeek] Prepared download queue for user '{User}' with {Count} items (expectedTrackCount={Expected})", candidate.Username, queue.Count, expectedTrackCount);

            if (minRequiredTracks > 0 && queue.Count < minRequiredTracks)
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
                if (cancellationToken.IsCancellationRequested) { return false; }
                var item = queue.Dequeue();
                var localPath = Path.Combine(targetDirectory, item.LocalFileName);
                var localDir = Path.GetDirectoryName(localPath);
                if (!string.IsNullOrWhiteSpace(localDir))
                    Directory.CreateDirectory(localDir);
                // Prefer source track number if available
                var displayTrack = item.SourceTrackNumber ?? (trackIndex + 1);
                logger.LogInformation(
                    "[SoulSeek] Downloading {File} to {Path} (track {Track}/{Total})",
                    item.FileName,
                    localPath,
                    displayTrack,
                    expectedTrackCount
                );

                await cache.UpdateMediaAvailabilityStatus(
                    artistId,
                    releaseFolderName,
                    displayTrack,
                    CachedMediaAvailabilityStatus.Downloading
                );

                try
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var noDataStopwatch = System.Diagnostics.Stopwatch.StartNew();
                    // Monitor for no-data outside of progress callback so we cancel even if no callbacks are fired
                    using var perFileCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    using var monitorCts = new CancellationTokenSource();
                    var lastDataTick = DateTime.UtcNow;
                    var monitorTask = Task.Run(async () =>
                    {
                        try
                        {
                            while (!monitorCts.IsCancellationRequested)
                            {
                                if ((DateTime.UtcNow - lastDataTick).TotalSeconds >= noDataTimeoutSec)
                                {
                                    try { perFileCts.Cancel(); } catch { }
                                    break;
                                }
                                await Task.Delay(500, monitorCts.Token);
                            }
                        }
                        catch { }
                    }, monitorCts.Token);
                    long lastBytes = 0;
                    var lastTick = DateTime.UtcNow;
                    double lastLoggedPercent = -25;
                    // Wire progress callback via TransferOptions (Soulseek.NET)
                    var options = new TransferOptions(
                        progressUpdated: (update) =>
                        {
                            try
                            {
                                var previousBytesTransferred = update.PreviousBytesTransferred;
                                var transfer = update.Transfer;
                                var received = transfer.BytesTransferred;
                                var total = transfer.Size;

                                var now = DateTime.UtcNow;
                                var seconds = Math.Max(0.001, (now - lastTick).TotalSeconds);
                                var delta = Math.Max(0, received - previousBytesTransferred);
                                var kbps = (delta / 1024.0) / seconds;
                                lastTick = now;

                                double percent = total > 0 ? (received * 100.0) / total : 0;
                                if (delta > 0)
                                {
                                    noDataStopwatch.Restart();
                                    lastDataTick = DateTime.UtcNow;
                                }
                                else if (noDataStopwatch.Elapsed.TotalSeconds >= noDataTimeoutSec)
                                {
                                    // Cancel transfer: no data for too long
                                    try { perFileCts.Cancel(); } catch { }
                                }
                                progress.Set(new DownloadProgress
                                {
                                    ArtistId = artistId,
                                    ReleaseFolderName = releaseFolderName,
                                    Status = DownloadStatus.Downloading,
                                    TotalTracks = expectedTrackCount,
                                    CompletedTracks = displayTrack,
                                    ArtistName = cachedRelease?.ArtistName ?? artistName,
                                    ReleaseTitle = cachedRelease?.Title ?? releaseTitle,
                                    CoverArtUrl = Features.ServerLibrary.Utils.LibraryAssetUrlFactory.CreateReleaseCoverArtUrl(artistId, releaseFolderName),
                                    CurrentTrackProgressPercent = percent,
                                    CurrentDownloadSpeedKbps = kbps,
                                });
                                if (logger.IsEnabled(LogLevel.Debug) && percent - lastLoggedPercent >= 25)
                                {
                                    lastLoggedPercent = percent;
                                    logger.LogDebug(
                                        "[SoulSeek] Progress {Percent:n0}% on track {Track}/{Total} at ~{Kbps:n0} KB/s",
                                        percent,
                                        trackIndex + 1,
                                        expectedTrackCount,
                                        kbps
                                    );
                                }
                            }
                            catch { }
                        }
                    );

                    try
                    {
                        await client.DownloadAsync(
                        item.Username,
                        item.FileName,
                        localPath,
                        options: options,
                        cancellationToken: perFileCts.Token
                        );
                    }
                    finally
                    {
                        try { monitorCts.Cancel(); } catch { }
                        try { await monitorTask; } catch { }
                    }
                    sw.Stop();
                    // Final push on completion
                    try
                    {
                        progress.Set(new DownloadProgress
                        {
                            ArtistId = artistId,
                            ReleaseFolderName = releaseFolderName,
                            Status = DownloadStatus.Downloading,
                            TotalTracks = expectedTrackCount,
                            CompletedTracks = displayTrack,
                            ArtistName = cachedRelease?.ArtistName ?? artistName,
                            ReleaseTitle = cachedRelease?.Title ?? releaseTitle,
                            CoverArtUrl = Features.ServerLibrary.Utils.LibraryAssetUrlFactory.CreateReleaseCoverArtUrl(artistId, releaseFolderName),
                            CurrentTrackProgressPercent = 100,
                            CurrentDownloadSpeedKbps = null,
                        });
                        try
                        {
                            var sizeBytes = new FileInfo(localPath).Length;
                            var avgKbps = sizeBytes > 0 ? (sizeBytes / 1024.0) / Math.Max(0.001, sw.Elapsed.TotalSeconds) : 0;
                            logger.LogInformation(
                                "[SoulSeek] Finished track {Track}/{Total} in {Seconds:n1}s at avg ~{AvgKbps:n0} KB/s",
                                displayTrack,
                                expectedTrackCount,
                                sw.Elapsed.TotalSeconds,
                                avgKbps
                            );
                        }
                        catch { }
                    }
                    catch { }
                }
                catch (OperationCanceledException ocex)
                {
                    // Distinguish between external cancellation and no-data timeout
                    if (cancellationToken.IsCancellationRequested)
                    {
                        logger.LogInformation("[SoulSeek] Download cancelled externally for {Artist}/{Folder}", artistId, releaseFolderName);
                        return false;
                    }
                    logger.LogWarning(ocex, "[SoulSeek] Download cancelled due to no-data timeout ({Seconds}s) for file {File}", noDataTimeoutSec, item.FileName);
                    userFailed = true;
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
                    displayTrack,
                    CachedMediaAvailabilityStatus.Processing
                );
                logger.LogDebug("[SoulSeek] Marked track {Track} as Processing", displayTrack);

                trackIndex++;
                try
                {
                    progress.SetTrackProgress(displayTrack, expectedTrackCount);
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
                try
                {
                    downloadHistory.Add(new DownloadHistoryItem(
                        DateTime.UtcNow,
                        artistId,
                        releaseFolderName,
                        artistName,
                        releaseTitle,
                        true,
                        null
                    ));
                }
                catch { }
                if (!downloadQueue.TryDequeue(out var nxt) || nxt is null)
                {
                    progress.Reset();
                }
                else
                {
                    // Reflect original priority if re-queued explicitly by worker; keep normal queue
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
        try
        {
            downloadHistory.Add(new DownloadHistoryItem(
                DateTime.UtcNow,
                artistId,
                releaseFolderName,
                artistName,
                releaseTitle,
                false,
                "All candidates failed"
            ));
        }
        catch { }
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

    private static int ComputeCandidateScore(
        SearchResponse response,
        string artistName,
        string releaseTitle,
        string? expectedYear,
        int expectedTrackCount,
        List<int> allowedOfficialCounts,
        List<int> allowedOfficialDigitalCounts)
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
        var discNumbers = new HashSet<int>();
        bool anyDiscMarker = false;

        int audio320Count = response.Files.Count(f =>
            f.Extension.Equals("mp3", StringComparison.OrdinalIgnoreCase) && f.BitRate == 320);

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

                // Detect disc markers in segments following album
                for (int j = idxAlbum + 1; j < segments.Length; j++)
                {
                    var s = segments[j];
                    var m = Regex.Match(s, @"\b(?:cd|disc|disk)[\s_-]*([0-9]+)?\b", RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        anyDiscMarker = true;
                        if (m.Groups[1].Success && int.TryParse(m.Groups[1].Value, out int d))
                        {
                            if (d > 0) discNumbers.Add(d);
                        }
                    }
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

        // Prefer responses with a digital/expected track count match
        if (allowedOfficialDigitalCounts is { Count: > 0 })
        {
            if (allowedOfficialDigitalCounts.Contains(audio320Count)) score += 120; // strong preference for digital
        }

        if (expectedTrackCount > 0)
        {
            if (audio320Count == expectedTrackCount) score += 100;
            else if (Math.Abs(audio320Count - expectedTrackCount) == 1) score += 40;
            else if (audio320Count > 0)
            {
                // mild closeness bonus within +/-2
                if (Math.Abs(audio320Count - expectedTrackCount) == 2) score += 15;
            }
        }

        // Penalize multi-disc structures. Not forbidden, but de-prioritize compared to single-disc/digital
        if (discNumbers.Count > 1)
        {
            score -= 100; // multiple disc folders detected
        }
        else if (anyDiscMarker)
        {
            score -= 50; // at least one disc folder present
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