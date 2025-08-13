using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Playlists.Db;
using MusicGQL.Features.Playlists.Subscription;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Writer;
using Path = System.IO.Path;

namespace MusicGQL.Features.Downloads.Services;

public class StartDownloadReleaseService(
    ServerLibraryCache cache,
    SoulSeekReleaseDownloader soulSeekReleaseDownloader,
    ServerLibraryJsonWriter writer,
    ILogger<StartDownloadReleaseService> logger,
    Features.Import.Services.MusicBrainzImportService mbImport,
    Features.Import.Services.LibraryReleaseImportService releaseImporter,
    HotChocolate.Subscriptions.ITopicEventSender eventSender,
    IDbContextFactory<EventDbContext> dbFactory,
    DownloadCancellationService cancellationService
)
{
    public async Task<(bool Success, string? ErrorMessage)> StartAsync(
        string artistId,
        string releaseFolderName,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "[StartDownload] Begin for {ArtistId}/{ReleaseFolder}",
            artistId,
            releaseFolderName
        );

        var release = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);

        if (release == null)
        {
            var msg = $"Release not found in cache: {artistId}/{releaseFolderName}";
            logger.LogWarning("[StartDownload] {Message}", msg);
            return (false, "Release not found in cache");
        }

        var artistName = release.ArtistName;
        var releaseTitle = release.Title;
        var targetDir = release.ReleasePath; // full path on disk

        logger.LogInformation(
            "[StartDownload] Resolved targetDir={TargetDir}, artistName='{Artist}', releaseTitle='{Title}'",
            targetDir,
            artistName,
            releaseTitle
        );

        // Pre-enrich release.json with possible track counts (digital/official) if missing
        try
        {
            var hasOfficial = release.JsonRelease?.PossibleNumberOfTracksInOfficialReleases?.Count > 0;
            var hasDigital = release.JsonRelease?.PossibleNumberOfTracksInOfficialDigitalReleases?.Count > 0;
            if (!hasOfficial || !hasDigital)
            {
                var artist = await cache.GetArtistByIdAsync(artistId);
                var mbArtistId = artist?.JsonArtist.Connections?.MusicBrainzArtistId;
                if (!string.IsNullOrWhiteSpace(mbArtistId))
                {
                    var rgs = await mbImport.GetArtistReleaseGroupsAsync(mbArtistId!);
                    // Prefer RGs whose titles are equivalent, exclude demos, prefer Album primary type, then earliest date
                    var candidates = rgs
                        .Where(rg => AreTitlesEquivalent(rg.Title, releaseTitle ?? string.Empty))
                        .Where(rg => !(rg.SecondaryTypes?.Any(t => t.Equals("Demo", StringComparison.OrdinalIgnoreCase)) ?? false))
                        .ToList();
                    if (candidates.Count == 0)
                    {
                        candidates = rgs
                            .Where(rg => (rg.Title ?? string.Empty).IndexOf(releaseTitle ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0)
                            .Where(rg => !(rg.SecondaryTypes?.Any(t => t.Equals("Demo", StringComparison.OrdinalIgnoreCase)) ?? false))
                            .ToList();
                    }
                    var match = candidates
                        .OrderByDescending(rg => string.Equals(rg.PrimaryType, "Album", StringComparison.OrdinalIgnoreCase))
                        .ThenBy(rg => SafeDateKey(rg.FirstReleaseDate))
                        .FirstOrDefault();
                    if (match is not null)
                    {
                        // Rebuild in place; the builder will compute possible track counts (official + digital)
                        var importResult = await releaseImporter.ImportReleaseGroupInPlaceAsync(
                            match.Id,
                            match.Title,
                            match.PrimaryType,
                            Path.GetDirectoryName(release.ReleasePath) ?? Path.Combine("./Library", artistId),
                            artistId,
                            release.FolderName
                        );
                        if (importResult.Success)
                        {
                            await cache.UpdateReleaseFromJsonAsync(artistId, releaseFolderName);
                            // Refresh local variable for subsequent logic
                            release = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);
                            logger.LogInformation("[StartDownload] Pre-enriched possible track counts from MusicBrainz for {ArtistId}/{Folder}", artistId, releaseFolderName);
                        }
                    }
                }
            }
        }
        catch (Exception enrichEx)
        {
            logger.LogDebug(enrichEx, "[StartDownload] Skipped pre-enrichment of possible track counts");
        }

        // Set status to Searching before starting
        await cache.UpdateReleaseDownloadStatus(
            artistId,
            releaseFolderName,
            CachedReleaseDownloadStatus.Searching
        );

        logger.LogInformation(
            "[StartDownload] Delegating to SoulSeek downloader for {Artist}/{Folder}",
            artistId,
            releaseFolderName
        );
        var token = cancellationService.CreateFor(artistId, releaseFolderName, cancellationToken);
        var ok = await soulSeekReleaseDownloader.DownloadReleaseAsync(
            artistId,
            releaseFolderName,
            artistName,
            releaseTitle,
            targetDir,
            token
        );
        if (!ok)
        {
            var msg = $"No suitable download found for {artistName} - {releaseTitle}";
            logger.LogWarning("[StartDownload] {Message}", msg);

            await cache.UpdateReleaseDownloadStatus(
                artistId,
                releaseFolderName,
                CachedReleaseDownloadStatus.NotFound
            );

            return (false, "No suitable download found");
        }

        var releaseJsonPath = Path.Combine(targetDir, "release.json");
        logger.LogInformation("[StartDownload] Updating JSON at {Path}", releaseJsonPath);

        if (File.Exists(releaseJsonPath))
        {
            try
            {
                logger.LogDebug("[StartDownload] release.json exists. Enumerating audio files for injection...");
                var audioFiles = Directory
                    .GetFiles(targetDir)
                    .Where(f =>
                        new[] { ".mp3", ".flac", ".wav", ".m4a", ".ogg" }.Contains(
                            Path.GetExtension(f).ToLowerInvariant()
                        )
                    )
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .Select(Path.GetFileName)
                    .ToList();

                logger.LogInformation(
                    "[StartDownload] Found {Count} audio files in {Dir}",
                    audioFiles.Count,
                    targetDir
                );

                await writer.UpdateReleaseAsync(
                    artistId,
                    releaseFolderName,
                    rel =>
                    {
                        if (rel.Tracks is null)
                            return;
                        // Build map from leading track number in filename -> filename
                        int ExtractLeadingNumber(string? name)
                        {
                            if (string.IsNullOrWhiteSpace(name)) return -1;
                            var span = name.AsSpan();
                            int pos = 0;
                            while (pos < span.Length && !char.IsDigit(span[pos])) pos++;
                            int start = pos;
                            while (pos < span.Length && char.IsDigit(span[pos])) pos++;
                            if (pos > start && int.TryParse(span.Slice(start, pos - start), out var n))
                            {
                                // Normalize 3+ digit disc+track encodings (e.g., 103 -> 3)
                                if (n > 99)
                                {
                                    var lastTwo = n % 100;
                                    if (lastTwo > 0) return lastTwo;
                                }
                                return n;
                            }
                            return -1;
                        }

                        var byTrackNo = new Dictionary<int, string>();
                        foreach (var f in audioFiles)
                        {
                            var n = ExtractLeadingNumber(f);
                            if (n > 0 && !byTrackNo.ContainsKey(n)) byTrackNo[n] = f;
                        }

                        foreach (var t in rel.Tracks)
                        {
                            if (byTrackNo.TryGetValue(t.TrackNumber, out var fname))
                            {
                                t.AudioFilePath = "./" + fname;
                            }
                        }
                    }
                );

                logger.LogInformation("[StartDownload] Updated release.json with audio file paths");

                // Reload just this release into cache so it reflects new JSON (preserves transient availability)
                logger.LogInformation(
                    "[StartDownload] Refreshing release in cache after JSON update..."
                );
                await cache.UpdateReleaseFromJsonAsync(artistId, releaseFolderName);

                // Now publish availability status updates to reflect current runtime state
                var relAfterCount = audioFiles.Count; // used for bounds below
                await Task.WhenAll(
                    Enumerable
                        .Range(0, relAfterCount)
                        .Select(i =>
                            cache.UpdateMediaAvailabilityStatus(
                                artistId,
                                releaseFolderName,
                                i + 1,
                                CachedMediaAvailabilityStatus.Available
                            )
                        )
                );
                logger.LogInformation("[StartDownload] Marked {Count} tracks as Available", relAfterCount);

                // Backfill playlist items that should now reference these tracks
                try
                {
                    await LinkPlaylistItemsForReleaseAsync(artistId, releaseFolderName);
                }
                catch (Exception backfillEx)
                {
                    logger.LogWarning(
                        backfillEx,
                        "[StartDownload] Failed linking playlist items for {ArtistId}/{Folder}",
                        artistId,
                        releaseFolderName
                    );
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "[StartDownload] Failed updating release.json for {ArtistId}/{Folder}",
                    artistId,
                    releaseFolderName
                );
                return (false, "Failed to update release.json after download");
            }
        }
        else
        {
            logger.LogWarning("[StartDownload] release.json not found at path: {Path}. Skipping JSON update.", releaseJsonPath);
        }

        await cache.UpdateReleaseDownloadStatus(
            artistId,
            releaseFolderName,
            CachedReleaseDownloadStatus.Idle
        );

        // Auto-refresh metadata now that audio exists
        try
        {
            var rel = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);
            var artist = await cache.GetArtistByIdAsync(artistId);
            var mbArtistId = artist?.JsonArtist.Connections?.MusicBrainzArtistId;
            if (!string.IsNullOrWhiteSpace(mbArtistId) && rel != null)
            {
                var rgs = await mbImport.GetArtistReleaseGroupsAsync(mbArtistId!);

                // Prefer RGs whose titles are equivalent, exclude demos, prefer Album primary type, then earliest date
                var candidates = rgs
                    .Where(rg => AreTitlesEquivalent(rg.Title, rel.Title ?? string.Empty))
                    .Where(rg => !(rg.SecondaryTypes?.Any(t => t.Equals("Demo", StringComparison.OrdinalIgnoreCase)) ?? false))
                    .ToList();

                if (candidates.Count == 0)
                {
                    // Fallback: loose contains check
                    candidates = rgs
                        .Where(rg => (rg.Title ?? string.Empty).IndexOf(rel.Title ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0)
                        .Where(rg => !(rg.SecondaryTypes?.Any(t => t.Equals("Demo", StringComparison.OrdinalIgnoreCase)) ?? false))
                        .ToList();
                }

                var match = candidates
                    .OrderByDescending(rg => string.Equals(rg.PrimaryType, "Album", StringComparison.OrdinalIgnoreCase))
                    .ThenBy(rg => SafeDateKey(rg.FirstReleaseDate))
                    .FirstOrDefault();

                if (match is not null)
                {
                    // Rebuild using this RG; builder will pick the best matching release considering local audio files
                    var importResult = await releaseImporter.ImportReleaseGroupInPlaceAsync(
                        match.Id,
                        match.Title,
                        match.PrimaryType,
                        Path.GetDirectoryName(rel.ReleasePath) ?? Path.Combine("./Library", artistId),
                        artistId,
                        rel.FolderName
                    );
                    if (importResult.Success)
                    {
                        await cache.UpdateReleaseFromJsonAsync(artistId, releaseFolderName);
                        // Publish metadata updated event
                        var updated = await cache.GetReleaseByArtistAndFolderAsync(
                            artistId,
                            releaseFolderName
                        );
                        if (updated != null)
                        {
                            await eventSender.SendAsync(
                                ServerLibrary.Subscription.LibrarySubscription.LibraryReleaseMetadataUpdatedTopic(
                                    artistId,
                                    releaseFolderName
                                ),
                                new ServerLibrary.Release(updated)
                            );

                            // Centralized release and artist notifications
                            await eventSender.SendAsync(
                                ServerLibrary.Subscription.LibrarySubscription.LibraryReleaseUpdatedTopic(
                                    artistId,
                                    releaseFolderName
                                ),
                                new ServerLibrary.Release(updated)
                            );
                            await eventSender.SendAsync(
                                ServerLibrary.Subscription.LibrarySubscription.LibraryArtistReleaseUpdatedTopic(
                                    artistId
                                ),
                                new ServerLibrary.Release(updated)
                            );
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[StartDownload] Auto-refresh after download failed");
        }

        // Finished
        logger.LogInformation("[StartDownload] Done for {ArtistId}/{ReleaseFolder}", artistId, releaseFolderName);
        return (true, null);
    }

    private async Task LinkPlaylistItemsForReleaseAsync(string artistId, string releaseFolderName)
    {
        var rel = await cache.GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName);
        if (rel == null || rel.Tracks == null || rel.Tracks.Count == 0)
            return;

        await using var db = await dbFactory.CreateDbContextAsync();

        // Candidates: items already linked to this artist but missing track mapping
        var candidates = await db
            .Set<DbPlaylistItem>()
            .Where(i =>
                i.LocalArtistId == artistId
                && (i.LocalReleaseFolderName == null || i.LocalReleaseFolderName == releaseFolderName)
                && i.LocalTrackNumber == null
                && i.SongTitle != null
            )
            .ToListAsync();

        if (candidates.Count == 0)
            return;

        var updated = new List<DbPlaylistItem>();
        foreach (var item in candidates)
        {
            // If item has a release title, require it to match the release title to reduce false positives
            if (!string.IsNullOrWhiteSpace(item.ReleaseTitle)
                && !AreTitlesEquivalent(item.ReleaseTitle!, rel.Title ?? string.Empty))
            {
                continue;
            }

            var title = item.SongTitle ?? string.Empty;
            var match = rel.Tracks
                .Where(t => !string.IsNullOrWhiteSpace(t.Title))
                .FirstOrDefault(t => AreTitlesEquivalent(t.Title!, title));

            if (match != null)
            {
                item.LocalReleaseFolderName = releaseFolderName;
                item.LocalTrackNumber = match.TrackNumber;
                updated.Add(item);
            }
        }

        if (updated.Count == 0)
            return;

        await db.SaveChangesAsync();

        // Notify subscribers for each updated item
        foreach (var item in updated)
        {
            try
            {
                await eventSender.SendAsync(
                    PlaylistSubscription.PlaylistItemUpdatedTopic(item.PlaylistId),
                    new PlaylistSubscription.PlaylistItemUpdatedMessage(item.PlaylistId, item.Id)
                );
            }
            catch { /* best effort */ }
        }
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

        var npa = NormalizeTitle(StripParentheses(a));
        var npb = NormalizeTitle(StripParentheses(b));
        return npa.Equals(npb, StringComparison.Ordinal);
    }

    private static int SafeDateKey(string? iso)
    {
        if (string.IsNullOrWhiteSpace(iso)) return int.MaxValue;
        var parts = iso.Split('-');
        int y = parts.Length > 0 && int.TryParse(parts[0], out var yy) ? yy : 9999;
        int m = parts.Length > 1 && int.TryParse(parts[1], out var mm) ? mm : 12;
        int d = parts.Length > 2 && int.TryParse(parts[2], out var dd) ? dd : 31;
        return y * 372 + m * 31 + d;
    }
}
