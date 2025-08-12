using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using MusicGQL.Types;
using MusicGQL.Features.Downloads.Services;

namespace MusicGQL.Features.Downloads;

[ExtendObjectType(typeof(Subscription))]
public record DownloadsSubscription
{
    public const string DownloadQueueUpdatedTopic = "DownloadQueueUpdated";
    public const string CurrentDownloadUpdatedTopic = "CurrentDownloadUpdated";
    public const string DownloadHistoryUpdatedTopic = "DownloadHistoryUpdated";

    public ValueTask<ISourceStream<DownloadQueueState>> SubscribeToDownloadQueueUpdated(
        [Service] ITopicEventReceiver receiver,
        CancellationToken cancellationToken
    ) => receiver.SubscribeAsync<DownloadQueueState>(DownloadQueueUpdatedTopic, cancellationToken);

    [Subscribe(With = nameof(SubscribeToDownloadQueueUpdated))]
    public DownloadQueueState DownloadQueueUpdated([EventMessage] DownloadQueueState state) => state;

    public ValueTask<ISourceStream<DownloadProgress?>> SubscribeToCurrentDownloadUpdated(
        [Service] ITopicEventReceiver receiver,
        CancellationToken cancellationToken
    ) => receiver.SubscribeAsync<DownloadProgress?>(CurrentDownloadUpdatedTopic, cancellationToken);

    [Subscribe(With = nameof(SubscribeToCurrentDownloadUpdated))]
    public DownloadProgress? CurrentDownloadUpdated([EventMessage] DownloadProgress? progress) => progress;

    public ValueTask<ISourceStream<List<DownloadHistoryItem>>> SubscribeToDownloadHistoryUpdated(
        [Service] ITopicEventReceiver receiver,
        CancellationToken cancellationToken
    ) => receiver.SubscribeAsync<List<DownloadHistoryItem>>(DownloadHistoryUpdatedTopic, cancellationToken);

    [Subscribe(With = nameof(SubscribeToDownloadHistoryUpdated))]
    public List<DownloadHistoryItem> DownloadHistoryUpdated([EventMessage] List<DownloadHistoryItem> history) => history;
}


