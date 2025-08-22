using Microsoft.Extensions.Hosting;

namespace MusicGQL.Features.Downloads.Services;

public class DownloadServicesStartupLogger(DownloadLogPathProvider logPathProvider, ILogger<DownloadServicesStartupLogger> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var services = new[] { "qbittorrent", "prowlarr", "sabnzbd" };
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
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Startup logger initialization failed");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}


