using Hqub.Lastfm;
using MusicGQL.Features.Recording;

namespace MusicGQL.Features.Recommendations;

public record RecommendationsSearchRoot
{
    public async Task<IEnumerable<LastFmArtist>> TopArtists([Service] LastfmClient lastfmClient)
    {
        var topArtists = await lastfmClient.Chart.GetTopArtistsAsync(1, 10);
        return topArtists.Select(artist => new LastFmArtist(artist));
    }
};
