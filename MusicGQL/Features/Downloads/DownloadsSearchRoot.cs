using System.Text;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db;
using MusicGQL.Db.Postgres;
using MusicGQL.Sagas.DownloadRelease;
using Newtonsoft.Json;

namespace MusicGQL.Features.Downloads;

public record DownloadsSearchRoot
{
    public async Task<IEnumerable<DownloadStatus>> All(
        [Service] EventDbContext dbContext,
        ILogger<DownloadsSearchRoot> logger
    )
    {
        var sagas = await dbContext.Sagas.AsNoTracking().ToListAsync();

        return sagas
            .Select(s =>
                JsonConvert.DeserializeObject<DownloadReleaseSagaData>(
                    Encoding.UTF8.GetString(s.Data)
                )
            )
            .OfType<DownloadReleaseSagaData>()
            .Select(sagaData => new DownloadStatus(sagaData));
    }
};
