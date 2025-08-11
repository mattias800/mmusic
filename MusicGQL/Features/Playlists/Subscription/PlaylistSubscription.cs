using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Subscription;

[ExtendObjectType(typeof(Types.Subscription))]
public record PlaylistSubscription
{
    public static string PlaylistItemUpdatedTopic(string playlistId) => $"PlaylistItemUpdated_{playlistId}";

    public ValueTask<ISourceStream<PlaylistItem>> SubscribeToPlaylistItemUpdated(
        [Service] ITopicEventReceiver receiver,
        [ID] string playlistId,
        CancellationToken cancellationToken
    ) => receiver.SubscribeAsync<PlaylistItem>(PlaylistItemUpdatedTopic(playlistId),
        cancellationToken);

    [Subscribe(With = nameof(SubscribeToPlaylistItemUpdated))]
    public PlaylistItem PlaylistItemUpdated([EventMessage] PlaylistItem item) =>
        item;
}