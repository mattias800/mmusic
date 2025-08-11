using MusicGQL.Features.Artists;
using MusicGQL.Features.MusicBrainz.Recording;
using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Integration.MusicBrainz;
using TrackSeries.FanArtTV.Client;
using Track = Hqub.Lastfm.Entities.Track;

namespace MusicGQL.Features.LastFm;

public record LastFmTrack([property: GraphQLIgnore] Track Model)
{
    [ID]
    public string Id() => Model.Url;

    public async Task<string?> MBID() => Model.MBID;

    public string Name() => Model.Name;

    public LastFmArtist Artist() => new(Model.Artist);

    public LastFmAlbum? Album() => Model.Album is null ? null : new(Model.Album);

    public async Task<ArtistImages?> Images(IFanArtTVClient fanartClient, ServerLibraryCache cache)
    {
        if (string.IsNullOrEmpty(Model.MBID))
        {
            var a = await cache.GetArtistByNameAsync(Model.Artist.Name);
            var mbId = a?.JsonArtist.Connections?.MusicBrainzArtistId;
            if (mbId is null)
            {
                return null;
            }

            var artist = await fanartClient.Music.GetArtistAsync(mbId);
            //return artist is null ? null : new(artist);
            return null;
        }

        try
        {
            var artist = await fanartClient.Music.GetArtistAsync(Model.MBID);
            //return artist is null ? null : new(artist);
            return null;
        }
        catch
        {
            return null;
        }
    }

    public long? PlayCount() => Model.Statistics.PlayCount;

    public string? Summary() => Model.Wiki?.Summary;

    public LastFmStatistics Statistics() => new(Model.Statistics);

    public async Task<ServerLibrary.Track?> Recording(ServerLibraryCache cache)
    {
        // var r = await service.SearchRecordingForArtistByArtistNameExactNameMatchAsync(
        //     Model.Name,
        //     Model.Artist.Name
        // );
        //
        // var f = r.FirstOrDefault();
        //
        // return f is null ? null : new ServerLibrary.Track(f);
        return null;
    }

    public async Task<MbRecording?> MusicBrainzRecording(MusicBrainzService service)
    {
        if (string.IsNullOrEmpty(Model.MBID))
        {
            return null;
        }

        var release = await service.GetRecordingByIdAsync(Model.MBID);
        return release is null ? null : new MbRecording(release);
    }
}
