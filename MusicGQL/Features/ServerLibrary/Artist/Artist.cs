using Hqub.Lastfm;
using MusicGQL.Features.LastFm;
using MusicGQL.Features.MusicBrainz.Artist;
using MusicGQL.Features.ServerLibrary.Artist.Db;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.ServerLibrary.Artist;

public record Artist([property: GraphQLIgnore] DbArtist Model)
{
    public string Id => Model.Id;
    public string Name => Model.Name;
    public string SortName => Model.SortName;
    public string? Gender => Model.Gender;

    public async Task<IEnumerable<LastFmTrack>> TopTracks([Service] LastfmClient lastfmClient)
    {
        try
        {
            var tracks = await lastfmClient.Artist.GetTopTracksByMbidAsync(Model.Id);

            return tracks
                .Where(t => t.MBID is not null)
                .OrderByDescending(t => t.Statistics.PlayCount)
                .Take(10)
                .Select(t => new LastFmTrack(t))
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    public async Task<MbArtist?> MusicBrainzArtist([Service] MusicBrainzService musicBrainzService)
    {
        var artist = await musicBrainzService.GetArtistByIdAsync(Model.Id);

        if (artist == null)
        {
            return null;
        }

        return new(artist);
    }
}
