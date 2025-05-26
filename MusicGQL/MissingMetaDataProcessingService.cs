using MusicGQL.Features.ServerLibrary.Import.Handlers;

namespace MusicGQL;

public class MissingMetaDataProcessingService(
    ProcessMissingMetaDataHandler processMissingMetaDataHandler
)
{
    public void ProcessMissingMetaData()
    {
        _ = DoIt();
    }

    private async Task DoIt()
    {
        await processMissingMetaDataHandler.Handle();
    }
}
