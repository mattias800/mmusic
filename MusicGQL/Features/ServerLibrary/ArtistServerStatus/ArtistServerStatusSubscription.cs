using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using MusicGQL.Types;

namespace MusicGQL.Features.ServerLibrary.ArtistServerStatus;

[ExtendObjectType(typeof(Subscription))]
public record ArtistServerStatusSubscription
{
    public static string ArtistServerStatusUpdatedTopic(string artistId) =>
        $"ArtistServerStatusUpdated_{artistId}";

    public ValueTask<ISourceStream<ArtistServerStatus>> SubscribeToArtistServerStatusUpdated(
        [Service] ITopicEventReceiver receiver,
        [ID] string artistId,
        CancellationToken cancellationToken
    ) =>
        receiver.SubscribeAsync<ArtistServerStatus>(
            ArtistServerStatusUpdatedTopic(artistId),
            cancellationToken
        );

    [Subscribe(With = nameof(SubscribeToArtistServerStatusUpdated))]
    public ArtistServerStatus ArtistServerStatusUpdated([EventMessage] ArtistServerStatus status) =>
        status;
}
