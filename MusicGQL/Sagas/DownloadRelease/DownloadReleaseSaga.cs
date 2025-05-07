using HotChocolate.Subscriptions;
using MusicGQL.Features.Downloads;
using MusicGQL.Types;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace MusicGQL.Sagas.DownloadRelease;

public class DownloadReleaseSaga(IBus bus, ITopicEventSender sender)
    : Saga<DownloadReleaseSagaData>,
        IAmInitiatedBy<DownloadReleaseQueuedEvent>,
        IHandleMessages<FoundReleaseInMusicBrainz>,
        IHandleMessages<FoundRecordingsForReleaseInMusicBrainz>,
        IHandleMessages<ReleaseNotFoundInMusicBrainz>,
        IHandleMessages<FoundReleaseDownload>
{
    protected override void CorrelateMessages(ICorrelationConfig<DownloadReleaseSagaData> config)
    {
        config.Correlate<DownloadReleaseQueuedEvent>(
            m => m.MusicBrainzReleaseId,
            s => s.MusicBrainzReleaseId
        );
        config.Correlate<FoundReleaseInMusicBrainz>(
            m => m.MusicBrainzReleaseId,
            s => s.MusicBrainzReleaseId
        );
        config.Correlate<FoundRecordingsForReleaseInMusicBrainz>(
            m => m.MusicBrainzReleaseId,
            s => s.MusicBrainzReleaseId
        );
        config.Correlate<FoundReleaseDownload>(
            m => m.MusicBrainzReleaseId,
            s => s.MusicBrainzReleaseId
        );
    }

    public async Task Handle(DownloadReleaseQueuedEvent message)
    {
        if (!IsNew)
        {
            return;
        }

        Data.StatusDescription = "Looking up release";

        await sender.SendAsync(nameof(Subscription.DownloadStarted), Data);
        await bus.Send(new LookupReleaseInMusicBrainz(message.MusicBrainzReleaseId));
    }

    public async Task Handle(FoundReleaseInMusicBrainz message)
    {
        // UI
        Data.ArtistName = message.Release.Credits.First().Artist.Name;
        Data.ReleaseName = message.Release.Title;
        Data.StatusDescription = "Looking up tracks";
        // Data
        Data.Release = message.Release;

        await sender.SendAsync(
            nameof(Subscription.DownloadStatusUpdated),
            new DownloadStatus(Data)
        );

        await bus.Send(
            new LookupRecordingsForReleaseInMusicBrainz(
                message.MusicBrainzReleaseId,
                message.Release
            )
        );
    }

    public async Task Handle(ReleaseNotFoundInMusicBrainz message)
    {
        Data.StatusDescription = "No download found";

        await sender.SendAsync(
            nameof(Subscription.DownloadStatusUpdated),
            new DownloadStatus(Data)
        );
        MarkAsComplete();
    }

    public async Task Handle(FoundRecordingsForReleaseInMusicBrainz message)
    {
        // UI
        Data.NumberOfTracks = message.Recordings.Count;
        Data.TracksDownloaded = 0;
        Data.StatusDescription = "Downloading...";
        // Data
        Data.Recordings = message.Recordings;

        await sender.SendAsync(
            nameof(Subscription.DownloadStatusUpdated),
            new DownloadStatus(Data)
        );

        foreach (var recording in message.Recordings)
        {
            await Task.Delay(3000);
            Data.TracksDownloaded++;
            await sender.SendAsync(
                nameof(Subscription.DownloadStatusUpdated),
                new DownloadStatus(Data)
            );
        }

        await bus.Send(
            new SearchReleaseDownload(
                Data.MusicBrainzReleaseId,
                message.Release,
                message.Recordings
            )
        );
    }

    public async Task Handle(FoundReleaseDownload message)
    {
        Data.StatusDescription = "Download found";
        await sender.SendAsync(
            nameof(Subscription.DownloadStatusUpdated),
            new DownloadStatus(Data)
        );
        MarkAsComplete();
    }
}
