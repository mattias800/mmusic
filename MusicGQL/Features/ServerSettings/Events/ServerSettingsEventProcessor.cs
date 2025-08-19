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
            case SoulSeekSearchTimeLimitUpdated timeLimitUpdated:
                await HandleSoulSeekSearchTimeLimitUpdated(timeLimitUpdated, dbContext);
                break;
            case SoulSeekNoDataTimeoutUpdated noDataTimeoutUpdated:
                await HandleSoulSeekNoDataTimeoutUpdated(noDataTimeoutUpdated, dbContext);
                break;
            case DownloadSlotCountUpdated slotCountUpdated:
                await HandleDownloadSlotCountUpdated(slotCountUpdated, dbContext);
                break;
            case PublicBaseUrlUpdated publicBaseUrlUpdated:
                await HandlePublicBaseUrlUpdated(publicBaseUrlUpdated, dbContext);
                break;
            case SoulSeekConnectionUpdated soulSeekConn:
                await HandleSoulSeekConnectionUpdated(soulSeekConn, dbContext);
                break;
            case ProwlarrSettingsUpdated prowlarr:
                await HandleProwlarrSettingsUpdated(prowlarr, dbContext);
                break;
            case QBittorrentSettingsUpdated qbit:
                await HandleQBittorrentSettingsUpdated(qbit, dbContext);
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

    private async Task HandleSoulSeekSearchTimeLimitUpdated(
        SoulSeekSearchTimeLimitUpdated updated,
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

        serverSettings.SoulSeekSearchTimeLimitSeconds = updated.NewSeconds;
        await dbContext.SaveChangesAsync();
    }

    private async Task HandleSoulSeekNoDataTimeoutUpdated(
        SoulSeekNoDataTimeoutUpdated updated,
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

        serverSettings.SoulSeekNoDataTimeoutSeconds = updated.NewSeconds;
        await dbContext.SaveChangesAsync();
    }

    private async Task HandleDownloadSlotCountUpdated(
        DownloadSlotCountUpdated updated,
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

        serverSettings.DownloadSlotCount = updated.NewSlotCount;
        await dbContext.SaveChangesAsync();
    }

    private async Task HandlePublicBaseUrlUpdated(
        PublicBaseUrlUpdated updated,
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

        serverSettings.PublicBaseUrl = updated.NewPublicBaseUrl;
        await dbContext.SaveChangesAsync();
    }

    private async Task HandleSoulSeekConnectionUpdated(
        SoulSeekConnectionUpdated updated,
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

        serverSettings.SoulSeekHost = updated.Host;
        serverSettings.SoulSeekPort = updated.Port;
        serverSettings.SoulSeekUsername = updated.Username;
        await dbContext.SaveChangesAsync();
    }

    private async Task HandleProwlarrSettingsUpdated(
        ProwlarrSettingsUpdated updated,
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

        serverSettings.ProwlarrBaseUrl = updated.BaseUrl;
        serverSettings.ProwlarrTimeoutSeconds = updated.TimeoutSeconds;
        serverSettings.ProwlarrMaxRetries = updated.MaxRetries;
        serverSettings.ProwlarrRetryDelaySeconds = updated.RetryDelaySeconds;
        serverSettings.ProwlarrTestConnectivityFirst = updated.TestConnectivityFirst;
        serverSettings.ProwlarrEnableDetailedLogging = updated.EnableDetailedLogging;
        serverSettings.ProwlarrMaxConcurrentRequests = updated.MaxConcurrentRequests;
        await dbContext.SaveChangesAsync();
    }

    private async Task HandleQBittorrentSettingsUpdated(
        QBittorrentSettingsUpdated updated,
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

        serverSettings.QBittorrentBaseUrl = updated.BaseUrl;
        serverSettings.QBittorrentUsername = updated.Username;
        serverSettings.QBittorrentSavePath = updated.SavePath;
        await dbContext.SaveChangesAsync();
    }
}
