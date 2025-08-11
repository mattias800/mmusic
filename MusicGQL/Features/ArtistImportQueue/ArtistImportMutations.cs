using MusicGQL.Features.Playlists.Import.Spotify;
using MusicGQL.Integration.Spotify;
using MusicGQL.Types;

namespace MusicGQL.Features.ArtistImportQueue;

public record EnqueueArtistsFromSpotifyPlaylistInput(string PlaylistId);

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
}


