using MusicGQL.Db.Postgres;
using MusicGQL.Features.Playlists.Events;
using MusicGQL.Integration.Spotify;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Import.Spotify.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class ImportSpotifyPlaylistMutation
{
    public async Task<ImportSpotifyPlaylistResult> ImportSpotifyPlaylistById(
        [Service] SpotifyService spotifyService,
        [Service] EventDbContext db,
        [Service] IHttpClientFactory httpClientFactory,
        [Service] MusicGQL.Features.Assets.ExternalAssetStorage assetStorage,
        string playlistId,
        Guid userId
    )
    {
        var spotifyPlaylist = await spotifyService.GetPlaylistDetailsAsync(playlistId);
        if (spotifyPlaylist == null)
        {
            return new ImportSpotifyPlaylistError("Spotify playlist not found");
        }

        // 1. Create Playlist event
        var playlistGuid = Guid.NewGuid();
        var createdPlaylistEvent = new CreatedPlaylist
        {
            PlaylistId = playlistGuid,
            ActorUserId = userId,
            Name = spotifyPlaylist.Name ?? string.Empty,
            Description = spotifyPlaylist.Description,
            CoverImageUrl = spotifyPlaylist.Images?.FirstOrDefault()?.Url,
        };

        db.Events.Add(createdPlaylistEvent);

        // 1b. Connect to external Spotify playlist for future sync
        db.Events.Add(
            new ConnectPlaylistToExternalPlaylist
            {
                PlaylistId = playlistGuid,
                ActorUserId = userId,
                ExternalService = ExternalServiceType.Spotify,
                ExternalPlaylistId = playlistId,
            }
        );

        // 2. Fetch tracks from Spotify
        var tracks = await spotifyService.GetTracksFromPlaylist(playlistId) ?? [];

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
                    playlistGuid,
                    track.Id,
                    coverUrl
                );
            }

            db.Events.Add(
                new SongAddedToPlaylist
                {
                    PlaylistId = playlistGuid,
                    RecordingId = track.Id, // Keep original RecordingId for backward compat
                    ActorUserId = userId,
                    Position = null, // Null means append to the end
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
        return new ImportSpotifyPlaylistSuccess(true);
    }
}

[UnionType]
public abstract record ImportSpotifyPlaylistResult;

public record ImportSpotifyPlaylistSuccess(bool Success) : ImportSpotifyPlaylistResult;

public record ImportSpotifyPlaylistError(string Message) : ImportSpotifyPlaylistResult;
