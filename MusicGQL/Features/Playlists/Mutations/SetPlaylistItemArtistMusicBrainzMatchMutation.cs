using HotChocolate.Subscriptions;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.ArtistImportQueue;
using MusicGQL.Features.ArtistImportQueue.Services;
using MusicGQL.Features.Import;
using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Writer;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Mutations;

[ExtendObjectType(typeof(Mutation))]
public sealed class SetPlaylistItemArtistMusicBrainzMatchMutation
{
    public async Task<SetPlaylistItemArtistMusicBrainzMatchResult> SetPlaylistItemArtistMusicBrainzMatch(
        SetPlaylistItemArtistMusicBrainzMatchInput input,
        [Service] EventDbContext db,
        [Service] ArtistImportQueueService queue
    )
    {
        var playlist = await db
            .Playlists.Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == input.PlaylistId);
        if (playlist is null)
        {
            return new SetPlaylistItemArtistMusicBrainzMatchNotFound("Playlist not found");
        }

        var item = playlist.Items.FirstOrDefault(i => i.Id == input.PlaylistItemId);
        if (item is null)
        {
            return new SetPlaylistItemArtistMusicBrainzMatchNotFound("Playlist item not found");
        }

        // Enqueue import instead of importing inline
        var queueItem = new ArtistImportQueueItem(item.ArtistName ?? string.Empty, item.SongTitle)
        {
            MusicBrainzArtistId = input.MusicBrainzArtistId,
            ExternalArtistId = item.ExternalArtistId,
            PlaylistId = playlist.Id,
            PlaylistItemId = item.Id,
        };
        queue.Enqueue(queueItem);

        return new SetPlaylistItemArtistMusicBrainzMatchSuccess(new PlaylistItem(item));
    }
}

public record SetPlaylistItemArtistMusicBrainzMatchInput(
    [property: ID] string PlaylistId,
    [property: ID] string PlaylistItemId,
    string MusicBrainzArtistId
);

[UnionType("SetPlaylistItemArtistMusicBrainzMatchResult")]
public abstract record SetPlaylistItemArtistMusicBrainzMatchResult;

public record SetPlaylistItemArtistMusicBrainzMatchSuccess(PlaylistItem PlaylistItem)
    : SetPlaylistItemArtistMusicBrainzMatchResult;

public record SetPlaylistItemArtistMusicBrainzMatchNotFound(string Message)
    : SetPlaylistItemArtistMusicBrainzMatchResult;

public record SetPlaylistItemArtistMusicBrainzMatchError(string Message)
    : SetPlaylistItemArtistMusicBrainzMatchResult;
