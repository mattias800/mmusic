using Rebus.Bus;
using Rebus.Handlers;

namespace MusicGQL.Sagas.DownloadRelease;

public sealed class SearchReleaseHandler(
    ILogger<SearchReleaseHandler> logger,
    IBus bus
) : IHandleMessages<SearchReleaseDownload>
{
    public async Task Handle(SearchReleaseDownload message)
    {
        logger.LogInformation("Searching download for release: {Release}", message.Release.Title);

        await Task.Delay(2000);

        logger.LogInformation("Found download for release: {Release}", message.Release.Title);

        await bus.Send(new FoundReleaseDownload(message.Release));
    }
}