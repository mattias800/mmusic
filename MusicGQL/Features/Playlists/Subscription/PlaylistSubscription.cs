using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Subscription;

[ExtendObjectType(typeof(Types.Subscription))]
public record PlaylistSubscription
{
    public static string PlaylistItemUpdatedTopic(string playlistId) => $"PlaylistItemUpdated_{playlistId}";

    public record PlaylistItemUpdatedMessage(string PlaylistId, string PlaylistItemId);

    public ValueTask<ISourceStream<PlaylistItemUpdatedMessage>> SubscribeToPlaylistItemUpdated(
        [Service] ITopicEventReceiver receiver,
        [ID] string playlistId,
        CancellationToken cancellationToken
    ) => receiver.SubscribeAsync<PlaylistItemUpdatedMessage>(PlaylistItemUpdatedTopic(playlistId),
        cancellationToken);

    [Subscribe(With = nameof(SubscribeToPlaylistItemUpdated))]
    public async Task<PlaylistItem> PlaylistItemUpdated(
        [EventMessage] PlaylistItemUpdatedMessage message,
        [Service] EventDbContext db
    )
    {
        var playlist = await db
            .Playlists
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == message.PlaylistId);

        var item = playlist?.Items.FirstOrDefault(i => i.Id == message.PlaylistItemId);
        if (item is null)
        {
            throw new GraphQLException($"Playlist item not found: {message.PlaylistItemId}");
        }
        return new PlaylistItem(item);
    }
}