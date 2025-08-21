using MusicGQL.Features.Downloads.Services;

namespace MusicGQL.Features.External.Downloads;

public class SoulSeekDownloadProvider(
    SoulSeekReleaseDownloader downloader,
    ServerSettings.ServerSettingsAccessor serverSettingsAccessor
) : IDownloadProvider
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
        return TryAsync();

        async Task<bool> TryAsync()
        {
            var settings = await serverSettingsAccessor.GetAsync();
            if (!settings.EnableSoulSeekDownloader)
                return false;

            return await downloader.DownloadReleaseAsync(
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
}


