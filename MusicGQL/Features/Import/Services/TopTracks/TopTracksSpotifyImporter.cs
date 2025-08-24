using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Integration.Spotify;

namespace MusicGQL.Features.Import.Services.TopTracks;

public class TopTracksSpotifyImporter(SpotifyService spotifyService) : ITopTracksImporter
{
    public async Task<List<JsonTopTrack>> GetTopTracksAsync(string spotifyArtistId, int take = 10)
    {
        var top = await spotifyService.GetArtistTopTracksAsync(spotifyArtistId) ?? [];
        return top.Take(take)
            .Select(t => new JsonTopTrack
            {
                Title = t.Name,
                ReleaseTitle = t.Album?.Name,
                CoverArt = null,
                // Spotify API doesn't provide play count in this endpoint
                PlayCount = null,
                TrackLength = (int?)t.DurationMs,
            })
            .ToList();
    }
}
