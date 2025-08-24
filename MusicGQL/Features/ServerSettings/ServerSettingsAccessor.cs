using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.ServerSettings.Db;

namespace MusicGQL.Features.ServerSettings;

public class ServerSettingsAccessor(IDbContextFactory<EventDbContext> dbFactory)
{
    public async Task<DbServerSettings> GetAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var s = await db.ServerSettings.FirstOrDefaultAsync(s =>
            s.Id == DefaultDbServerSettingsProvider.ServerSettingsSingletonId
        );
        return s ?? DefaultDbServerSettingsProvider.GetDefault();
    }
}
