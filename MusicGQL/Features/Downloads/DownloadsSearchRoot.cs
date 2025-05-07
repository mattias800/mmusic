using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db;
using MusicGQL.Sagas.DownloadRelease;
using Newtonsoft.Json;

namespace MusicGQL.Features.Downloads;

public record DownloadsSearchRoot
{
    [ID]
    public string GetId() => "DownloadSearchRoot";

    public async Task<IEnumerable<DownloadStatus>> All([Service] EventDbContext dbContext,
        ILogger<DownloadsSearchRoot> logger)
    {
        var sagas = await dbContext.Sagas.AsNoTracking().ToListAsync();

        return sagas
            .Select(s =>
            {
                logger.LogInformation("Parsing: {Data}", Encoding.UTF8.GetString(s.Data));
                return JsonConvert.DeserializeObject<DownloadReleaseSagaData>(Encoding.UTF8.GetString(s.Data));
            }).OfType<DownloadReleaseSagaData>()
            .Select(sagaData => new DownloadStatus(sagaData));
    }
};