using MusicGQL.Features.ServerLibrary.Artist.Handlers;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Handlers;
using Rebus.Bus;

namespace MusicGQL;

public class ScheduledTaskPublisher(
    IServiceScopeFactory scopeFactory,
    IBus bus,
    ILogger<ScheduledTaskPublisher> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Scheduled task publisher started");

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Scheduled task publisher doing work");

            try
            {
                using var scope = scopeFactory.CreateScope();
                var processMissingArtistsInServerLibraryHandler =
                    scope.ServiceProvider.GetRequiredService<ProcessMissingArtistsInServerLibraryHandler>();
                var processMissingReleaseGroupsInServerLibraryHandler =
                    scope.ServiceProvider.GetRequiredService<ProcessMissingReleaseGroupsInServerLibraryHandler>();

                await processMissingArtistsInServerLibraryHandler.Handle(new());
                await processMissingReleaseGroupsInServerLibraryHandler.Handle(new());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while processing scheduled task");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
