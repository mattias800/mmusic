using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models.Events.Playlists;
using MusicGQL.Integration.Spotify;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Import.Spotify.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class ImportSpotifyPlaylistMutation
{
    public async Task<ImportSpotifyPlaylistResult> ImportSpotifyPlaylistById(
        [Service] SpotifyService spotifyService,
        [Service] EventDbContext db,
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

        // 2. Fetch tracks from Spotify
        var tracks = await spotifyService.GetTracksFromPlaylist(playlistId) ?? [];

        foreach (var track in tracks)
        {
            if (track == null || string.IsNullOrEmpty(track.Id))
                continue; // Skip if track or track.Id is null

            db.Events.Add(
                new SongAddedToPlaylist
                {
                    PlaylistId = playlistGuid,
                    RecordingId = track.Id, // Spotify Track ID
                    ActorUserId = userId,
                    Position = null, // Null means append to the end
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
