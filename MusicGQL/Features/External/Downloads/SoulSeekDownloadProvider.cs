using MusicGQL.Features.Downloads.Services;

namespace MusicGQL.Features.External.Downloads;

public class SoulSeekDownloadProvider(SoulSeekReleaseDownloader downloader) : IDownloadProvider
{
    public Task<bool> TryDownloadReleaseAsync(
        string artistId,
        string releaseFolderName,
        string artistName,
        string releaseTitle,
        string targetDirectory,
        IReadOnlyList<int> allowedOfficialCounts,
        IReadOnlyList<int> allowedOfficialDigitalCounts,
        CancellationToken cancellationToken
    )
    {
        return downloader.DownloadReleaseAsync(
            artistId,
            releaseFolderName,
            artistName,
            releaseTitle,
            targetDirectory,
            allowedOfficialCounts.ToList(),
            allowedOfficialDigitalCounts.ToList(),
            cancellationToken
        );
    }
}


