using MusicGQL.Features.ServerLibrary.Artist.Handlers;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Handlers;

namespace MusicGQL;

public class MissingMetaDataProcessingService(
    ProcessMissingArtistsInServerLibraryHandler processMissingArtistsInServerLibraryHandler,
    ProcessMissingReleaseGroupsInServerLibraryHandler processMissingReleaseGroupsInServerLibraryHandler
)
{
    public void ProcessMissingMetaData()
    {
        _ = DoIt();
    }

    private async Task DoIt()
    {
        await processMissingArtistsInServerLibraryHandler.Handle();
        await processMissingReleaseGroupsInServerLibraryHandler.Handle();
    }
}
