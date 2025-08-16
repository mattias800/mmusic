using MusicGQL.Features.ServerSettings.Db;
using MusicGQL.Features.ServerSettings.Events;
using MusicGQL.Features.Downloads.Services;
using MusicGQL.Db.Postgres;
using MusicGQL.EventProcessor;

namespace MusicGQL.Features.ServerSettings.Commands;

public class UpdateDownloadSlotCountHandler(
    EventDbContext dbContext,
    EventProcessorWorker eventProcessorWorker,
    IDownloadSlotManager slotManager
)
{
    public record Command(Guid UserId, int NewSlotCount);

    public enum Result
    {
        Success,
        InvalidSlotCount
    }

    public async Task<Result> Handle(Command command)
    {
        // Validate slot count
        if (command.NewSlotCount < 1 || command.NewSlotCount > 10)
        {
            return Result.InvalidSlotCount;
        }

        // Get current settings
        var settings = await dbContext.ServerSettings.FindAsync(DefaultDbServerSettingsProvider.ServerSettingsSingletonId);
        if (settings == null)
        {
            // Create default settings if none exist
            settings = DefaultDbServerSettingsProvider.GetDefault();
            dbContext.ServerSettings.Add(settings);
        }

        // Update slot count
        settings.DownloadSlotCount = command.NewSlotCount;
        await dbContext.SaveChangesAsync();

        // Add event to database
        dbContext.Events.Add(
            new DownloadSlotCountUpdated
            {
                ActorUserId = Guid.Empty, // TODO: Get actual user ID
                NewSlotCount = command.NewSlotCount,
            }
        );

        await dbContext.SaveChangesAsync();
        await eventProcessorWorker.ProcessEvents();

        // Update slot manager configuration
        await slotManager.UpdateSlotConfigurationAsync(command.NewSlotCount, CancellationToken.None);

        return Result.Success;
    }
}
