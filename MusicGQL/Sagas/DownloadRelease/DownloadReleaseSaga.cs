using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace MusicGQL.Sagas.DownloadRelease;

public class DownloadReleaseSaga(IBus bus) :
    Saga<DownloadReleaseSagaData>,
    IAmInitiatedBy<DownloadReleaseQueuedEvent>,
    IHandleMessages<FoundReleaseInMusicBrainz>,
    IHandleMessages<FoundReleaseDownload>
{
    protected override void CorrelateMessages(ICorrelationConfig<DownloadReleaseSagaData> config)
    {
        config.Correlate<DownloadReleaseQueuedEvent>(m => m.MusicBrainzReleaseId, s => s.MusicBrainzReleaseId);
    }

    public async Task Handle(DownloadReleaseQueuedEvent message)
    {
        if (!IsNew)
        {
            return;
        }

        await bus.Send(new LookupReleaseInMusicBrainz(message.MusicBrainzReleaseId));
    }

    public async Task Handle(FoundReleaseInMusicBrainz message)
    {
        Data.Release = message.Release;
        await bus.Send(new SearchReleaseDownload(message.Release));
    }

    public Task Handle(FoundReleaseDownload message)
    {
        MarkAsComplete();
        return Task.CompletedTask;
    }
}