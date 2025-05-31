using MusicGQL.Db.Postgres;

namespace MusicGQL.Features.Downloads;

public record DownloadsSearchRoot
{
    public async Task<IEnumerable<DownloadStatus>> All(
        [Service] EventDbContext dbContext,
        ILogger<DownloadsSearchRoot> logger
    )
    {
        return [];
    }
};
