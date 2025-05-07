using HotChocolate.Subscriptions;
using MusicGQL.Types;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace MusicGQL.Sagas.DownloadRelease;

public class DownloadReleaseSaga(IBus bus, ITopicEventSender sender) :
    Saga<DownloadReleaseSagaData>,
    IAmInitiatedBy<DownloadReleaseQueuedEvent>,
    IHandleMessages<FoundReleaseInMusicBrainz>,
    IHandleMessages<ReleaseNotFoundInMusicBrainz>,
    IHandleMessages<FoundReleaseDownload>
{
    protected override void CorrelateMessages(ICorrelationConfig<DownloadReleaseSagaData> config)
    {
        config.Correlate<DownloadReleaseQueuedEvent>(m => m.MusicBrainzReleaseId, s => s.MusicBrainzReleaseId);
        config.Correlate<FoundReleaseInMusicBrainz>(m => m.MusicBrainzReleaseId, s => s.MusicBrainzReleaseId);
        config.Correlate<FoundReleaseDownload>(m => m.MusicBrainzReleaseId, s => s.MusicBrainzReleaseId);
    }

    public async Task Handle(DownloadReleaseQueuedEvent message)
    {
        if (!IsNew)
        {
            return;
        }

        await sender.SendAsync(nameof(Subscription.DownloadStarted), Data);
        await bus.Send(new LookupReleaseInMusicBrainz(message.MusicBrainzReleaseId));
    }

    public async Task Handle(FoundReleaseInMusicBrainz message)
    {
        Data.Release = message.Release;
        await sender.SendAsync(nameof(Subscription.DownloadStatusUpdated), Data);
        await bus.Send(new SearchReleaseDownload(message.MusicBrainzReleaseId, message.Release));
    }

    public async Task Handle(ReleaseNotFoundInMusicBrainz message)
    {
        await sender.SendAsync(nameof(Subscription.DownloadStatusUpdated), Data);
        MarkAsComplete();
    }

    public async Task Handle(FoundReleaseDownload message)
    {
        await sender.SendAsync(nameof(Subscription.DownloadStatusUpdated), Data);
        MarkAsComplete();
    }
}