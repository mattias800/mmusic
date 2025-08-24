using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.ArtistImportQueue.Services;
using MusicGQL.Features.Playlists.Events;
using MusicGQL.Integration.Spotify;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Import.Spotify.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class ImportSpotifyPlaylistMutation
{
    public async Task<ImportSpotifyPlaylistResult> ImportSpotifyPlaylist(
        SpotifyService spotifyService,
        EventDbContext db,
        Assets.ExternalAssetStorage assetStorage,
        EventProcessor.EventProcessorWorker eventProcessorWorker,
        ArtistImportQueueService artistImportQueue,
        ServerLibrary.Cache.ServerLibraryCache cache,
        ILogger<ImportSpotifyPlaylistMutation> logger,
        ImportSpotifyPlaylistInput input
    )
    {
        var spotifyPlaylist = await spotifyService.GetPlaylistDetailsAsync(input.PlaylistId);
        if (spotifyPlaylist == null)
        {
            return new ImportSpotifyPlaylistError("Spotify playlist not found");
        }

        // 1. Create Playlist event
        var playlistId = Guid.NewGuid().ToString();
        var createdPlaylistEvent = new CreatedPlaylist
        {
            PlaylistId = playlistId,
            ActorUserId = input.UserId,
            Name = spotifyPlaylist.Name ?? string.Empty,
            Description = spotifyPlaylist.Description,
            CoverImageUrl = spotifyPlaylist.Images?.FirstOrDefault()?.Url,
        };

        db.Events.Add(createdPlaylistEvent);

        // 1b. Connect to external Spotify playlist for future sync
        db.Events.Add(
            new ConnectPlaylistToExternalPlaylist
            {
                PlaylistId = playlistId,
                ActorUserId = input.UserId,
                ExternalService = ExternalServiceType.Spotify,
                ExternalPlaylistId = input.PlaylistId,
            }
        );

        // 2. Fetch tracks from Spotify
        var tracks = await spotifyService.GetTracksFromPlaylist(input.PlaylistId) ?? [];

        foreach (var track in tracks)
        {
            if (track == null || string.IsNullOrEmpty(track.Id))
                continue; // Skip if track or track.Id is null

            var coverUrl = track.Album?.Images?.FirstOrDefault()?.Url;
            string? localCoverUrl = null;
            if (!string.IsNullOrWhiteSpace(coverUrl))
            {
                // Attempt to cache cover art locally for portability
                localCoverUrl = await assetStorage.SaveCoverImageForPlaylistTrackAsync(
                    playlistId,
                    track.Id,
                    coverUrl
                );
            }

            db.Events.Add(
                new SongAddedToPlaylist
                {
                    PlaylistId = playlistId,
                    PlaylistItemId = Guid.NewGuid().ToString(),
                    ActorUserId = input.UserId,
                    AtIndex = null, // Null means append to the end
                    ExternalService = ExternalServiceType.Spotify,
                    ExternalTrackId = track.Id,
                    ExternalAlbumId = track.Album?.Id,
                    ExternalArtistId = track.Artists?.FirstOrDefault()?.Id,
                    SongTitle = track.Name,
                    ArtistName = track.Artists?.FirstOrDefault()?.Name,
                    ReleaseTitle = track.Album?.Name,
                    ReleaseType = track.Album?.AlbumType?.ToString(),
                    TrackLengthMs = track.DurationMs,
                    CoverImageUrl = coverUrl,
                    LocalCoverImageUrl = localCoverUrl,
                }
            );
        }

        await db.SaveChangesAsync();

        await eventProcessorWorker.ProcessEvents();

        var playlist = await db
            .Playlists.Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == playlistId);

        if (playlist is null)
        {
            return new ImportSpotifyPlaylistError("Playlist not found after update");
        }

        // Auto-enqueue missing artists from this imported playlist
        try
        {
            var missing =
                new List<(string ArtistName, string? ExternalArtistId, string? SongTitle)>();
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

            if (missing.Count > 0)
            {
                var items = missing
                    .GroupBy(x =>
                        (
                            NameLower: x.ArtistName.ToLowerInvariant(),
                            ExternalId: x.ExternalArtistId ?? string.Empty
                        )
                    )
                    .Select(g =>
                    {
                        var any = g.First();
                        var chosenSong = g.Select(x => x.SongTitle)
                            .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
                        return new ArtistImportQueue.ArtistImportQueueItem(
                            any.ArtistName,
                            chosenSong
                        )
                        {
                            ExternalArtistId = string.IsNullOrWhiteSpace(any.ExternalArtistId)
                                ? null
                                : any.ExternalArtistId,
                        };
                    })
                    .ToList();

                logger.LogInformation(
                    "Auto-enqueuing {Count} missing artists for imported Spotify playlist {PlaylistId}",
                    items.Count,
                    playlist.Id
                );
                artistImportQueue.Enqueue(items);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to auto-enqueue missing artists for imported Spotify playlist {PlaylistId}",
                playlist.Id
            );
        }

        return new ImportSpotifyPlaylistSuccess(new Playlist(playlist));
    }
}

[UnionType]
public abstract record ImportSpotifyPlaylistResult;

public record ImportSpotifyPlaylistSuccess(Playlist Playlist) : ImportSpotifyPlaylistResult;

public record ImportSpotifyPlaylistError(string Message) : ImportSpotifyPlaylistResult;

public record ImportSpotifyPlaylistInput(string PlaylistId, Guid UserId);
