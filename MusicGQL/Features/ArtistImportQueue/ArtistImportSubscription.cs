using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using MusicGQL.Types;

namespace MusicGQL.Features.ArtistImportQueue;

[ExtendObjectType(typeof(Subscription))]
public record ArtistImportSubscription
{
    public const string ArtistImportQueueUpdatedTopic = "ArtistImportQueueUpdated";
    public const string CurrentArtistImportUpdatedTopic = "CurrentArtistImportUpdated";

    public ValueTask<ISourceStream<ArtistImportQueueState>> SubscribeToArtistImportQueueUpdated(
        [Service] ITopicEventReceiver receiver,
        CancellationToken cancellationToken
    ) => receiver.SubscribeAsync<ArtistImportQueueState>(ArtistImportQueueUpdatedTopic, cancellationToken);

    [Subscribe(With = nameof(SubscribeToArtistImportQueueUpdated))]
    public ArtistImportQueueState ArtistImportQueueUpdated([EventMessage] ArtistImportQueueState state) => state;

    public ValueTask<ISourceStream<ArtistImportProgress>> SubscribeToCurrentArtistImportUpdated(
        [Service] ITopicEventReceiver receiver,
        CancellationToken cancellationToken
    ) => receiver.SubscribeAsync<ArtistImportProgress>(CurrentArtistImportUpdatedTopic, cancellationToken);

    [Subscribe(With = nameof(SubscribeToCurrentArtistImportUpdated))]
    public ArtistImportProgress CurrentArtistImportUpdated([EventMessage] ArtistImportProgress progress) => progress;
}


