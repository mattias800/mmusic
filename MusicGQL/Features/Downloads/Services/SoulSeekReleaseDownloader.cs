using System.Text.RegularExpressions;
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

        // Normalize query (e.g., remove commas) to improve matching
        string normArtist = NormalizeForSearch(artistName);
        string normTitle = NormalizeForSearch(releaseTitle);
        var query = $"{normArtist} - {normTitle}".Trim();
        logger.LogInformation("[SoulSeek] Normalized search query: {Query}", query);

        var result = await client.SearchAsync(new SearchQuery(query));

        // Order candidate responses similarly to BestResponseFinder but return an ordered list
        var orderedCandidates = result.Responses
            .Where(r => r.FileCount > 0)
            .Where(Sagas.Util.BestResponseFinder.HasCorrectMediaType)
            .OrderBy(r => r.QueueLength)
            .ThenBy(r => r.HasFreeUploadSlot)
            .ThenByDescending(r => r.UploadSpeed)
            .ToList();

        if (orderedCandidates.Count == 0)
        {
            logger.LogWarning(
                "[SoulSeek] No results for: {Artist} - {Release}",
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
            }

            if (!userFailed)
            {
                logger.LogInformation(
                    "[SoulSeek] Download complete: {Artist} - {Release} (user {User})",
                    artistName,
                    releaseTitle,
                    candidate.Username
                );
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
        return false;
    }

    private static string NormalizeForSearch(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        // Keep only letters, numbers, and spaces; collapse multiple spaces
        // \p{L} = any kind of letter from any language; \p{N} = any kind of numeric character
        var alnumSpaceOnly = Regex.Replace(input, "[^\\p{L}\\p{N}\\s]", " ");
        return Regex.Replace(alnumSpaceOnly, "\\s+", " ").Trim();
    }
}
