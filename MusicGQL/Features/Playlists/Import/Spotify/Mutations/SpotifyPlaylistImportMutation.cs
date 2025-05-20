using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models.Events.Playlists;
using MusicGQL.Integration.Spotify;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Import.Spotify.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class SpotifyPlaylistImportMutation
{
    public async Task<bool> ImportSpotifyPlaylistById(
        [Service] SpotifyService spotifyService,
        [Service] EventDbContext db,
        string playlistId,
        Guid userId
    )
    {
        // 0. Fetch Playlist details from Spotify
        var spotifyPlaylist = await spotifyService.GetPlaylistDetailsAsync(playlistId);
        if (spotifyPlaylist == null)
        {
            // Handle case where playlist is not found or error occurs
            // For now, let's return false or throw an exception
            return false; // Or throw new System.Exception("Spotify playlist not found");
        }

        // 1. Create Playlist event
        var playlistGuid = Guid.NewGuid();
        var createdPlaylistEvent = new CreatedPlaylist
        {
            PlaylistId = playlistGuid,
            ActorUserId = userId,
            Name = spotifyPlaylist.Name ?? string.Empty,
            SpotifyPlaylistId = spotifyPlaylist.Id,
            Description = spotifyPlaylist.Description,
            CoverImageUrl = spotifyPlaylist.Images?.FirstOrDefault()?.Url,
        };
        db.Events.Add(createdPlaylistEvent);

        // 2. Fetch tracks from Spotify
        var tracks = await spotifyService.GetTracksFromPlaylist(playlistId) ?? [];
        int position = 0;

        foreach (var track in tracks)
        {
            if (track == null || string.IsNullOrEmpty(track.Id))
                continue; // Skip if track or track.Id is null

            db.Events.Add(
                new SongAddedToPlaylist
                {
                    PlaylistId = playlistGuid,
                    RecordingId = track.Id, // Spotify Track ID
                    TrackName = track.Name ?? string.Empty,
                    ArtistName = track.Artists?.FirstOrDefault()?.Name ?? string.Empty,
                    ActorUserId = userId,
                    Position = position++,
                }
            );
        }

        await db.SaveChangesAsync();
        return true;
    }
}
