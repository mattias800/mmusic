using MusicGQL.Features.ServerLibrary.Import.Handlers;

namespace MusicGQL;

public class MissingMetaDataProcessingService(IServiceScopeFactory scopeFactory)
{
    public void ProcessMissingMetaData()
    {
        _ = Task.Run(async () => await DoItAsync());
    }

    private async Task DoItAsync()
    {
        using (var scope = scopeFactory.CreateScope())
        {
            var handler = scope.ServiceProvider.GetRequiredService<ProcessMissingMetaDataHandler>();
            await handler.ProcessMissingArtists();
            await handler.ProcessMissingReleaseGroups();
        }
    }
}
