using HotChocolate.Subscriptions;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
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
        [Service] LibraryImportService importService,
        [Service] ServerLibrary.Cache.ServerLibraryCache cache,
        [Service] ITopicEventSender events,
        [Service] ServerLibraryJsonWriter writer
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

        // Step 1: Import the artist from MusicBrainz (creates artist.json and sets MB connection)
        var importResult = await importService.ImportArtistByMusicBrainzIdAsync(input.MusicBrainzArtistId);
        if (!importResult.Success || string.IsNullOrWhiteSpace(importResult.ArtistJson?.Id))
        {
            return new SetPlaylistItemArtistMusicBrainzMatchError(
                importResult.ErrorMessage ?? "Failed to import artist");
        }

        var localArtistId = importResult.ArtistJson!.Id!;

        // Ensure cache is updated so subsequent lookups reflect the newly imported artist
        await cache.UpdateCacheAsync();

        // Step 2: If the playlist item has an external artist id (e.g., Spotify), write that connection into artist.json
        if (!string.IsNullOrWhiteSpace(item.ExternalArtistId) &&
            item.ExternalService == Features.Playlists.Events.ExternalServiceType.Spotify)
        {
            await writer.UpdateArtistAsync(localArtistId, artist =>
            {
                artist.Connections ??= new Features.ServerLibrary.Json.JsonArtistServiceConnections();
                artist.Connections.SpotifyId = item.ExternalArtistId;
            });
        }

        // Persist any artist.json connection updates by refreshing cache
        await cache.UpdateCacheAsync();

        // Step 3: For every playlist item in the system that references that same external artist (e.g. Spotify)
        // and is missing a local artist reference, update to the newly imported local artist
        if (!string.IsNullOrWhiteSpace(item.ExternalArtistId) &&
            item.ExternalService == Features.Playlists.Events.ExternalServiceType.Spotify)
        {
            var affectedPlaylists = await db
                .Playlists.Include(p => p.Items)
                .Where(p => p.Items.Any(i => i.LocalArtistId == null && i.ExternalArtistId == item.ExternalArtistId))
                .ToListAsync();

            foreach (var pl in affectedPlaylists)
            {
                foreach (var it in pl.Items.Where(i =>
                             i.LocalArtistId == null && i.ExternalArtistId == item.ExternalArtistId))
                {
                    it.LocalArtistId = localArtistId;
                    await events.SendAsync(
                        Features.Playlists.Subscription.PlaylistSubscription.PlaylistItemUpdatedTopic(pl.Id),
                        new Features.Playlists.Subscription.PlaylistSubscription.PlaylistItemUpdatedMessage(pl.Id, it.Id)
                    );
                }
            }

            await db.SaveChangesAsync();
        }

        // Also ensure the current item is linked if it was missing
        if (item.LocalArtistId == null)
        {
            item.LocalArtistId = localArtistId;
            await db.SaveChangesAsync();
            await events.SendAsync(
                Features.Playlists.Subscription.PlaylistSubscription.PlaylistItemUpdatedTopic(playlist.Id),
                new Features.Playlists.Subscription.PlaylistSubscription.PlaylistItemUpdatedMessage(playlist.Id, item.Id)
            );
        }

        // Step 4: Start import process of all releases for that artist (ImportArtist already imports eligible releases).
        // If you want to force another pass, you could call ImportArtistReleasesAsync here. We rely on ImportArtistByMusicBrainzIdAsync.

        return new SetPlaylistItemArtistMusicBrainzMatchSuccess(new Features.Playlists.PlaylistItem(item));
    }
}

public record SetPlaylistItemArtistMusicBrainzMatchInput(
    [property: ID] string PlaylistId,
    [property: ID] string PlaylistItemId,
    string MusicBrainzArtistId
);

[UnionType("SetPlaylistItemArtistMusicBrainzMatchResult")]
public abstract record SetPlaylistItemArtistMusicBrainzMatchResult;

public record SetPlaylistItemArtistMusicBrainzMatchSuccess(Features.Playlists.PlaylistItem PlaylistItem)
    : SetPlaylistItemArtistMusicBrainzMatchResult;

public record SetPlaylistItemArtistMusicBrainzMatchNotFound(string Message)
    : SetPlaylistItemArtistMusicBrainzMatchResult;

public record SetPlaylistItemArtistMusicBrainzMatchError(string Message)
    : SetPlaylistItemArtistMusicBrainzMatchResult;