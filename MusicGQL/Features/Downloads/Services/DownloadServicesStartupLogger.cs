using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MusicGQL.Features.External.Downloads.Prowlarr.Configuration;
using MusicGQL.Features.External.Downloads.QBittorrent.Configuration;
using MusicGQL.Features.External.Downloads.Sabnzbd.Configuration;
using MusicGQL.Features.External.SoulSeek.Integration;

namespace MusicGQL.Features.Downloads.Services;

public class DownloadServicesStartupLogger(
    DownloadLogPathProvider logPathProvider,
    ILogger<DownloadServicesStartupLogger> logger,
    MusicGQL.Features.ServerSettings.ServerSettingsAccessor serverSettingsAccessor,
    IOptions<ProwlarrOptions> prowlarrOptions,
    IOptions<QBittorrentOptions> qbittorrentOptions,
    IOptions<SabnzbdOptions> sabnzbdOptions,
    IOptions<SoulSeekConnectOptions> soulSeekOptions
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var services = new[] { "qbittorrent", "prowlarr", "sabnzbd", "soulseek" };
            foreach (var name in services)
            {
                try
                {
                    var path = await logPathProvider.GetServiceLogFilePathAsync(name, cancellationToken);
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        using var _ = new DownloadLogger(path!);
                    }
                }
                catch { }
            }

            var settings = await serverSettingsAccessor.GetAsync();

            // qBittorrent
            try
            {
                var path = await logPathProvider.GetServiceLogFilePathAsync("qbittorrent", cancellationToken);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    using var qblog = new DownloadLogger(path!);
                    var qOpt = qbittorrentOptions.Value;
                    qblog.Info($"Startup: enabled={settings.EnableQBittorrentDownloader} baseUrlPresent={!string.IsNullOrWhiteSpace(qOpt.BaseUrl)} usernamePresent={!string.IsNullOrWhiteSpace(qOpt.Username)} savePathPresent={!string.IsNullOrWhiteSpace(qOpt.SavePath)}");
                }
                logger.LogInformation("[Startup] qBittorrent: enabled={Enabled} baseUrlPresent={Base} usernamePresent={User} savePathPresent={Save}",
                    settings.EnableQBittorrentDownloader,
                    !string.IsNullOrWhiteSpace(qbittorrentOptions.Value.BaseUrl),
                    !string.IsNullOrWhiteSpace(qbittorrentOptions.Value.Username),
                    !string.IsNullOrWhiteSpace(qbittorrentOptions.Value.SavePath));
            }
            catch { }

            // SABnzbd
            try
            {
                var path = await logPathProvider.GetServiceLogFilePathAsync("sabnzbd", cancellationToken);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    using var sablog = new DownloadLogger(path!);
                    var sOpt = sabnzbdOptions.Value;
                    sablog.Info($"Startup: enabled={settings.EnableSabnzbdDownloader} baseUrlPresent={!string.IsNullOrWhiteSpace(sOpt.BaseUrl)} apiKeyPresent={!string.IsNullOrWhiteSpace(sOpt.ApiKey)} category={(sOpt.Category ?? string.Empty)}");
                }
                logger.LogInformation("[Startup] SABnzbd: enabled={Enabled} baseUrlPresent={Base} apiKeyPresent={Key}",
                    settings.EnableSabnzbdDownloader,
                    !string.IsNullOrWhiteSpace(sabnzbdOptions.Value.BaseUrl),
                    !string.IsNullOrWhiteSpace(sabnzbdOptions.Value.ApiKey));
            }
            catch { }

            // Prowlarr
            try
            {
                var path = await logPathProvider.GetServiceLogFilePathAsync("prowlarr", cancellationToken);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    using var plog = new DownloadLogger(path!);
                    var pOpt = prowlarrOptions.Value;
                    plog.Info($"Startup: baseUrlPresent={!string.IsNullOrWhiteSpace(pOpt.BaseUrl)} apiKeyPresent={!string.IsNullOrWhiteSpace(pOpt.ApiKey)} timeoutSec={pOpt.TimeoutSeconds} retries={pOpt.MaxRetries}");
                }
                logger.LogInformation("[Startup] Prowlarr: baseUrlPresent={Base} apiKeyPresent={Key}",
                    !string.IsNullOrWhiteSpace(prowlarrOptions.Value.BaseUrl),
                    !string.IsNullOrWhiteSpace(prowlarrOptions.Value.ApiKey));
            }
            catch { }

            // SoulSeek
            try
            {
                var path = await logPathProvider.GetServiceLogFilePathAsync("soulseek", cancellationToken);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    using var slog = new DownloadLogger(path!);
                    var so = soulSeekOptions.Value;
                    slog.Info($"Startup: downloaderEnabled={settings.EnableSoulSeekDownloader} librarySharingEnabled={settings.SoulSeekLibrarySharingEnabled} host={settings.SoulSeekHost} port={settings.SoulSeekPort} usernamePresent={!string.IsNullOrWhiteSpace(settings.SoulSeekUsername)} passwordPresent={!string.IsNullOrWhiteSpace(so.Password)}");
                }
                logger.LogInformation("[Startup] SoulSeek: downloaderEnabled={Enabled} librarySharingEnabled={Share} host={Host} port={Port} usernamePresent={User}",
                    settings.EnableSoulSeekDownloader,
                    settings.SoulSeekLibrarySharingEnabled,
                    settings.SoulSeekHost,
                    settings.SoulSeekPort,
                    !string.IsNullOrWhiteSpace(settings.SoulSeekUsername));
            }
            catch { }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Startup logger initialization failed");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}


