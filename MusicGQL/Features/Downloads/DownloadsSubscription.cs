using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using MusicGQL.Types;
using MusicGQL.Features.Downloads.Services;

namespace MusicGQL.Features.Downloads;

[ExtendObjectType(typeof(Subscription))]
public record DownloadsSubscription
{
    public const string DownloadQueueUpdatedTopic = "DownloadQueueUpdated";
    public const string DownloadHistoryUpdatedTopic = "DownloadHistoryUpdated";
    public const string SlotProgressUpdatedTopic = "SlotProgressUpdated";
    public const string SlotStatusUpdatedTopic = "SlotStatusUpdated";

    public ValueTask<ISourceStream<DownloadQueueState>> SubscribeToDownloadQueueUpdated(
        [Service] ITopicEventReceiver receiver,
        CancellationToken cancellationToken
    ) => receiver.SubscribeAsync<DownloadQueueState>(DownloadQueueUpdatedTopic, cancellationToken);

    [Subscribe(With = nameof(SubscribeToDownloadQueueUpdated))]
    public DownloadQueueState DownloadQueueUpdated([EventMessage] DownloadQueueState state) => state;

    public ValueTask<ISourceStream<List<DownloadHistoryItem>>> SubscribeToDownloadHistoryUpdated(
        [Service] ITopicEventReceiver receiver,
        CancellationToken cancellationToken
    ) => receiver.SubscribeAsync<List<DownloadHistoryItem>>(DownloadHistoryUpdatedTopic, cancellationToken);

    [Subscribe(With = nameof(SubscribeToDownloadHistoryUpdated))]
    public List<DownloadHistoryItem> DownloadHistoryUpdated([EventMessage] List<DownloadHistoryItem> history) => history;

    public ValueTask<ISourceStream<SlotProgressUpdate>> SubscribeToSlotProgressUpdated(
        [Service] ITopicEventReceiver receiver,
        CancellationToken cancellationToken
    ) => receiver.SubscribeAsync<SlotProgressUpdate>(SlotProgressUpdatedTopic, cancellationToken);

    [Subscribe(With = nameof(SubscribeToSlotProgressUpdated))]
    public SlotProgressUpdate SlotProgressUpdated([EventMessage] SlotProgressUpdate update) => update;

    public ValueTask<ISourceStream<SlotStatusUpdate>> SubscribeToSlotStatusUpdated(
        [Service] ITopicEventReceiver receiver,
        CancellationToken cancellationToken
    ) => receiver.SubscribeAsync<SlotStatusUpdate>(SlotStatusUpdatedTopic, cancellationToken);

    [Subscribe(With = nameof(SubscribeToSlotStatusUpdated))]
    public SlotStatusUpdate SlotStatusUpdated([EventMessage] SlotStatusUpdate update) => update;
}


