using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using MusicGQL.Types;

namespace MusicGQL.Features.Downloads;

[ExtendObjectType(typeof(Subscription))]
public record DownloadsSubscription
{
    public const string DownloadQueueUpdatedTopic = "DownloadQueueUpdated";
    public const string CurrentDownloadUpdatedTopic = "CurrentDownloadUpdated";

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
}


