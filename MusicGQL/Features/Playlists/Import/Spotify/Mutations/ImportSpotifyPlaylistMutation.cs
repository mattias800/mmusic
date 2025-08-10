using MusicGQL.Db.Postgres;
using MusicGQL.Features.Playlists.Events;
using MusicGQL.Integration.Spotify;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Import.Spotify.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class ImportSpotifyPlaylistMutation
{
    public async Task<ImportSpotifyPlaylistResult> ImportSpotifyPlaylist(
        ImportSpotifyPlaylistInput input,
        [Service] SpotifyService spotifyService,
        [Service] EventDbContext db,
        [Service] IHttpClientFactory httpClientFactory,
        [Service] Assets.ExternalAssetStorage assetStorage
    )
    {
        var spotifyPlaylist = await spotifyService.GetPlaylistDetailsAsync(input.PlaylistId);
        if (spotifyPlaylist == null)
        {
            return new ImportSpotifyPlaylistError("Spotify playlist not found");
        }

        // 1. Create Playlist event
        var playlistGuid = Guid.NewGuid();
        var createdPlaylistEvent = new CreatedPlaylist
        {
            PlaylistId = playlistGuid,
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
                PlaylistId = playlistGuid,
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
                    ActorUserId = input.UserId,
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

        // Return the created playlist projection
        var created = new Db.DbPlaylist
        {
            Id = playlistGuid,
            UserId = input.UserId,
            Name = spotifyPlaylist.Name,
            Description = spotifyPlaylist.Description,
            CoverImageUrl = spotifyPlaylist.Images?.FirstOrDefault()?.Url,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
        };
        // Note: We don't have a post-projection read here; we construct a minimal model for return.
        return new ImportSpotifyPlaylistSuccess(new Playlist(created));
    }
}

[UnionType]
public abstract record ImportSpotifyPlaylistResult;

public record ImportSpotifyPlaylistSuccess(Playlist Playlist) : ImportSpotifyPlaylistResult;

public record ImportSpotifyPlaylistError(string Message) : ImportSpotifyPlaylistResult;

public record ImportSpotifyPlaylistInput(string PlaylistId, Guid UserId);
