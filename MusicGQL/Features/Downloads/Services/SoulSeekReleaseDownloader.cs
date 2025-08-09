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

        var best = Sagas.Util.BestResponseFinder.GetBestSearchResponse(result.Responses.ToList());
        if (best == null)
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

        var queue = DownloadQueueFactory.Create(best);
        int trackIndex = 0;
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

            await client.DownloadAsync(item.Username, item.FileName, localPath);

            await cache.UpdateMediaAvailabilityStatus(
                artistId,
                releaseFolderName,
                trackIndex + 1,
                CachedMediaAvailabilityStatus.Processing
            );

            trackIndex++;
        }

        logger.LogInformation(
            "[SoulSeek] Download complete: {Artist} - {Release}",
            artistName,
            releaseTitle
        );
        return true;
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
