using Hqub.Lastfm;
using MusicGQL.Features.LastFm;

namespace MusicGQL.Features.Recommendations;

public record RecommendationsSearchRoot
{
    public async Task<IEnumerable<LastFmArtist>> TopArtists([Service] LastfmClient lastfmClient)
    {
        var topArtists = await lastfmClient.Chart.GetTopArtistsAsync(1, 20);
        return topArtists.Select(artist => new LastFmArtist(artist));
    }

    public async Task<IEnumerable<LastFmTrack>> TopTracks([Service] LastfmClient lastfmClient)
    {
        var topTracks = await lastfmClient.Chart.GetTopTracksAsync(1, 20);
        return topTracks.Select(t => new LastFmTrack(t));
    }

    public async Task<IEnumerable<LastFmTag>> TopTags([Service] LastfmClient lastfmClient)
    {
        var topTags = await lastfmClient.Chart.GetTopTagsAsync(1, 20);
        return topTags.Select(t => new LastFmTag(t));
    }
}
