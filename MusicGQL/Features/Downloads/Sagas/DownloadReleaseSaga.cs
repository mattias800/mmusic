using HotChocolate.Subscriptions;
using MusicGQL.Features.Downloads.Sagas.Util;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;
using Soulseek;

namespace MusicGQL.Features.Downloads.Sagas;

public class DownloadReleaseSaga(
    IBus bus,
    ITopicEventSender sender,
    ISoulseekClient client,
    ILogger<DownloadReleaseSaga> logger
)
    : Saga<DownloadReleaseSagaData>,
        IAmInitiatedBy<DownloadReleaseQueuedEvent>,
        IHandleMessages<FoundReleaseInMusicBrainz>,
        IHandleMessages<FoundRecordingsForReleaseInMusicBrainz>,
        IHandleMessages<ReleaseNotFoundInMusicBrainz>
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
    }

    public async Task Handle(DownloadReleaseQueuedEvent message)
    {
        if (!IsNew)
        {
            return;
        }

        Data.StatusDescription = "Looking up release";

        await sender.SendAsync(nameof(DownloadSubscription.DownloadStarted), Data);
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
            nameof(DownloadSubscription.DownloadStatusUpdated),
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
            nameof(DownloadSubscription.DownloadStatusUpdated),
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

        var artistName = message.Release.Credits.First().Artist.Name;
        var releaseTitle = message.Release.Title;

        logger.LogInformation("Searching download for release: {Release}", releaseTitle);

        var download = await client.SearchAsync(new SearchQuery($"{artistName} - {releaseTitle}"));
        var firstResponse = BestResponseFinder.GetBestSearchResponse(download.Responses.ToList());

        if (firstResponse is null)
        {
            logger.LogWarning("No download found for release: {Release}", releaseTitle);
            await bus.Send(new NoRecordingsFoundInMusicBrainz(message.MusicBrainzReleaseId));
            MarkAsComplete();
            return;
        }

        logger.LogInformation("Found download for release: {Release}", releaseTitle);

        Data.DownloadQueue = DownloadQueueFactory.Create(firstResponse);
        Data.StatusDescription = "Found release";

        await sender.SendAsync(
            nameof(DownloadSubscription.DownloadStatusUpdated),
            new DownloadStatus(Data)
        );

        while (Data.DownloadQueue.Any())
        {
            var item = Data.DownloadQueue.Dequeue();

            logger.LogInformation("Downloading {FileName}", item.FileName);

            await client.DownloadAsync(item.Username, item.FileName, item.LocalFileName);
            Data.TracksDownloaded++;

            await sender.SendAsync(
                nameof(DownloadSubscription.DownloadStatusUpdated),
                new DownloadStatus(Data)
            );

            logger.LogInformation("Downloading {FileName} DONE", item.FileName);
        }

        MarkAsComplete();
    }
}
