namespace MusicGQL;

public class ScheduledTaskPublisher(
    IServiceScopeFactory scopeFactory,
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
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while processing scheduled task");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
