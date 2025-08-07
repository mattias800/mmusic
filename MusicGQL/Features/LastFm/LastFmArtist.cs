using Hqub.Lastfm;
using MusicGQL.Features.MusicBrainz.Artist;
using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Integration.MusicBrainz;
using TrackSeries.FanArtTV.Client;

namespace MusicGQL.Features.LastFm;

public record LastFmArtist([property: GraphQLIgnore] Hqub.Lastfm.Entities.Artist Model)
{
    [ID]
    public string Id => Model.Url;

    public string Name => Model.Name;
    public LastFmStatistics Statistics => new(Model.Statistics);
    public string? Summary => Model.Biography.Summary;

    public async Task<Artist?> Artist(ServerLibraryCache cache)
    {
        var a = await cache.GetArtistByNameAsync(Model.Name);
        return a is null ? null : new(a);
    }

    public async Task<ArtistImages?> Images(
        IFanArtTVClient fanartClient,
        MusicBrainzService service
    )
    {
        try
        {
            var a = await service.SearchArtistByNameAsync(Model.Name);
            var id = a.FirstOrDefault()?.Id;
            if (id is null)
            {
                return null;
            }

            var artist = await fanartClient.Music.GetArtistAsync(id);
            // return artist is null ? null : new(artist);
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<IEnumerable<LastFmTrack>> TopTracks(LastfmClient lastfmClient)
    {
        try
        {
            var tracks = await lastfmClient.Artist.GetTopTracksByMbidAsync(Model.MBID);

            return tracks.Take(20).Select(t => new LastFmTrack(t)).ToList();
        }
        catch
        {
            return [];
        }
    }

    public async Task<MbArtist?> MusicBrainzArtist(MusicBrainzService service)
    {
        var artists = await service.SearchArtistByNameAsync(Model.Name);
        var a = artists.FirstOrDefault();
        return a is null ? null : new MbArtist(a);
    }
}
