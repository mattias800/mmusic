using Hqub.Lastfm;
using MusicGQL.Features.ServerLibrary.Json;

namespace MusicGQL.Features.Import.Services.TopTracks;

public class TopTracksLastFmImporter(LastfmClient lastfmClient) : ITopTracksImporter
{
    public async Task<List<JsonTopTrack>> GetTopTracksAsync(string mbArtistId, int take = 10)
    {
        var top = await lastfmClient.Artist.GetTopTracksByMbidAsync(mbArtistId);
        if (top == null)
        {
            return [];
        }

        return top
            .Take(take)
            .Select(t => new JsonTopTrack
            {
                Title = t.Name,
                ReleaseTitle = t.Album?.Name,
                // CoverArt is set later when matched to local release
                CoverArt = null,
                PlayCount = t.Statistics?.PlayCount,
                TrackLength = t.Duration,
            })
            .ToList();
    }
}


