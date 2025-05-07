using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db;
using MusicGQL.Sagas.DownloadRelease;

namespace MusicGQL.Features.Downloads;

public record DownloadsSearchRoot
{
    [ID]
    public string GetId() => "DownloadSearchRoot";

    public async Task<IEnumerable<DownloadStatus>> All([Service] EventDbContext dbContext)
    {
        var sagas = await dbContext.Sagas.AsNoTracking().ToListAsync();

        return sagas
            .Select(s => JsonSerializer.Deserialize<DownloadReleaseSagaData>(s.Data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })).OfType<DownloadReleaseSagaData>()
            .Select(sagaData => new DownloadStatus(sagaData));
    }
};