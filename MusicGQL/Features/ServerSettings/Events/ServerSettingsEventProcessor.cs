using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models;
using MusicGQL.Features.ServerSettings.Db;

namespace MusicGQL.Features.ServerSettings.Events;

public class ServerSettingsEventProcessor
{
    public async Task ProcessEvent(Event ev, EventDbContext dbContext)
    {
        switch (ev)
        {
            case LibraryPathUpdated libraryPathUpdated:
                await HandleLibraryPathUpdated(libraryPathUpdated, dbContext);
                break;
        }
    }

    private async Task HandleLibraryPathUpdated(
        LibraryPathUpdated libraryPathUpdated,
        EventDbContext dbContext
    )
    {
        var serverSettings = dbContext.ServerSettings.FirstOrDefault(s =>
            s.Id == DefaultDbServerSettingsProvider.ServerSettingsSingletonId
        );

        if (serverSettings is null)
        {
            serverSettings = DefaultDbServerSettingsProvider.GetDefault();
            dbContext.ServerSettings.Add(serverSettings);
        }

        serverSettings.LibraryPath = libraryPathUpdated.NewLibraryPath;
        await dbContext.SaveChangesAsync();
    }
}
