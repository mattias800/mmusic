using MusicGQL.Features.External.SoulSeek.Integration;
using Soulseek;
using Directory = System.IO.Directory;
using Path = System.IO.Path;

namespace MusicGQL.Features.Downloads.Services;

public class SoulSeekReleaseDownloader(
    SoulSeekService service,
    ISoulseekClient client,
    ILogger<SoulSeekReleaseDownloader> logger
)
{
    public async Task<bool> DownloadReleaseAsync(
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
        var result = await client.SearchAsync(new SearchQuery($"{artistName} - {releaseTitle}"));

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

        var queue = Sagas.Util.DownloadQueueFactory.Create(best);
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
            await client.DownloadAsync(item.Username, item.FileName, localPath);
        }

        logger.LogInformation(
            "[SoulSeek] Download complete: {Artist} - {Release}",
            artistName,
            releaseTitle
        );
        return true;
    }
}
