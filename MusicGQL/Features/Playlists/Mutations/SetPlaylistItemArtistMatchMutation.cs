using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.ArtistImportQueue;
using MusicGQL.Features.ArtistImportQueue.Services;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Mutations;

[ExtendObjectType(typeof(Mutation))]
public sealed class SetPlaylistItemArtistMatchMutation
{
    public async Task<SetPlaylistItemArtistMatchResult> SetPlaylistItemArtistMatch(
        SetPlaylistItemArtistMatchInput input,
        [Service] EventDbContext db,
        [Service] ArtistImportQueueService queue
    )
    {
        var playlist = await db
            .Playlists.Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == input.PlaylistId);
        if (playlist is null)
        {
            return new SetPlaylistItemArtistMatchNotFound("Playlist not found");
        }

        var item = playlist.Items.FirstOrDefault(i => i.Id == input.PlaylistItemId);
        if (item is null)
        {
            return new SetPlaylistItemArtistMatchNotFound("Playlist item not found");
        }

        item.ExternalArtistId = input.ExternalArtistId;
        await db.SaveChangesAsync();

        // enqueue this artist for import using provided external reference
        if (!string.IsNullOrWhiteSpace(item.ArtistName))
        {
            var qi = new ArtistImportQueueItem(item.ArtistName, item.SongTitle)
            {
                ExternalArtistId = item.ExternalArtistId,
            };
            queue.Enqueue(qi);
        }

        return new SetPlaylistItemArtistMatchSuccess(new PlaylistItem(item));
    }
}

public record SetPlaylistItemArtistMatchInput(
    [property: ID] string PlaylistId,
    [property: ID] string PlaylistItemId,
    string ExternalArtistId
);

[UnionType("SetPlaylistItemArtistMatchResult")]
public abstract record SetPlaylistItemArtistMatchResult;

public record SetPlaylistItemArtistMatchSuccess(PlaylistItem PlaylistItem)
    : SetPlaylistItemArtistMatchResult;

public record SetPlaylistItemArtistMatchNotFound(string Message) : SetPlaylistItemArtistMatchResult;
