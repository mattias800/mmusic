using MusicGQL.Features.MusicBrainz.Recording;
using MusicGQL.Features.ServerLibrary.Artist;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Integration.Neo4j;
using TrackSeries.FanArtTV.Client;
using Recording = MusicGQL.Features.ServerLibrary.Recording.Recording;
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

    public async Task<ArtistImages?> Images(
        IFanArtTVClient fanartClient,
        ServerLibraryService service
    )
    {
        if (string.IsNullOrEmpty(Model.MBID))
        {
            var a = await service.SearchArtistByNameAsync(Model.Artist.Name);
            var id = a.FirstOrDefault()?.Id;
            if (id is null)
            {
                return null;
            }

            var artist = await fanartClient.Music.GetArtistAsync(id);
            return artist is null ? null : new(artist);
        }

        try
        {
            var artist = await fanartClient.Music.GetArtistAsync(Model.MBID);
            return artist is null ? null : new(artist);
        }
        catch
        {
            return null;
        }
    }

    public long? PlayCount() => Model.Statistics.PlayCount;

    public string? Summary() => Model.Wiki?.Summary;

    public LastFmStatistics Statistics() => new(Model.Statistics);

    public async Task<Recording?> Recording(ServerLibraryService service)
    {
        var r = await service.SearchRecordingForArtistByArtistNameExactNameMatchAsync(
            Model.Name,
            Model.Artist.Name
        );

        var f = r.FirstOrDefault();

        return f is null ? null : new Recording(f);
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
