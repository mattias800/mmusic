using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.ArtistImportQueue.Services;
using MusicGQL.Integration.Spotify;
using MusicGQL.Types;
using Microsoft.Extensions.Logging;

namespace MusicGQL.Features.ArtistImportQueue.Mutations;

[ExtendObjectType(typeof(Mutation))]
public sealed class ArtistImportMutations
{
    public async Task<bool> EnqueueArtistsFromSpotifyPlaylist(
        EnqueueArtistsFromSpotifyPlaylistInput input,
        [Service] SpotifyService spotifyService,
        [Service] ArtistImportQueueService queue
    )
    {
        var tracks = await spotifyService.GetTracksFromPlaylist(input.PlaylistId) ?? [];
        var uniqueArtists = tracks
            .SelectMany(t => t.Artists?.Select(a => a.Name) ?? [])
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var items = uniqueArtists.Select(name => new ArtistImportQueueItem(name, null));
        queue.Enqueue(items);
        return true;
    }

    public bool EnqueueArtist(
        string artistName,
        string? songTitle,
        [Service] ArtistImportQueueService queue
    )
    {
        queue.Enqueue(new ArtistImportQueueItem(artistName, songTitle));
        return true;
    }

    public bool RemoveArtistImportJob(
        string queueKey,
        [Service] ArtistImportQueueService queue
    )
    {
        return queue.TryRemove(queueKey);
    }

    public async Task<bool> EnqueueMissingArtistsFromPlaylist(
        [ID] string playlistId,
        [Service] EventDbContext db,
        [Service] ArtistImportQueueService queue,
        [Service] ServerLibrary.Cache.ServerLibraryCache cache,
        [Service] ILogger<ArtistImportMutations> logger
    )
    {
        logger.LogInformation("EnqueueMissingArtistsFromPlaylist requested for playlist {PlaylistId}", playlistId);
        var playlist = await db
            .Playlists.Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == playlistId);
        if (playlist is null)
        {
            logger.LogWarning("Playlist {PlaylistId} not found when enqueuing missing artists", playlistId);
            return false;
        }

        var missing = new List<(string ArtistName, string? ExternalArtistId, string? SongTitle)>();
        foreach (var it in playlist.Items)
        {
            if (!string.IsNullOrWhiteSpace(it.ArtistName))
            {
                var exists = await cache.GetArtistByNameAsync(it.ArtistName);
                if (exists is null)
                {
                    missing.Add((it.ArtistName, it.ExternalArtistId, it.SongTitle));
                }
            }
        }

        logger.LogInformation(
            "Playlist {PlaylistId} has {TotalItems} items, {MissingCount} artists missing from server library",
            playlistId,
            playlist.Items.Count,
            missing.Count
        );

        if (missing.Count > 0)
        {
            // Log details at Debug to avoid overly chatty info logs
            logger.LogDebug(
                "Missing artists detail: {Missing}",
                string.Join(
                    "; ",
                    missing.Select(m => $"Name='{m.ArtistName}' ExtId='{m.ExternalArtistId ?? ""}' Song='{m.SongTitle ?? ""}'")
                )
            );
        }

        if (missing.Count == 0)
        {
            logger.LogInformation("No missing artists to enqueue for playlist {PlaylistId}", playlistId);
            return true;
        }

        var items = missing
            .GroupBy(x => (NameLower: x.ArtistName.ToLowerInvariant(), ExternalId: x.ExternalArtistId ?? string.Empty))
            .Select(g =>
            {
                var any = g.First();
                var chosenSong = g.Select(x => x.SongTitle).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
                return new ArtistImportQueueItem(any.ArtistName, chosenSong)
                {
                    ExternalArtistId = string.IsNullOrWhiteSpace(any.ExternalArtistId) ? null : any.ExternalArtistId
                };
            });
        var itemList = items.ToList();
        logger.LogInformation("Enqueuing {Count} unique artist imports for playlist {PlaylistId}", itemList.Count, playlistId);
        queue.Enqueue(itemList);
        return true;
    }
}

public record EnqueueArtistsFromSpotifyPlaylistInput(string PlaylistId);