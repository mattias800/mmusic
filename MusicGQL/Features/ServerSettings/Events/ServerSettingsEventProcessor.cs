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
            case DownloadPathUpdated downloadPathUpdated:
                await HandleDownloadPathUpdated(downloadPathUpdated, dbContext);
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

        serverSettings.LibraryPath = libraryPathUpdated.NewPath;
        await dbContext.SaveChangesAsync();
    }

    private async Task HandleDownloadPathUpdated(
        DownloadPathUpdated downloadPathUpdated,
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

        serverSettings.DownloadPath = downloadPathUpdated.NewPath;
        await dbContext.SaveChangesAsync();
    }
}
