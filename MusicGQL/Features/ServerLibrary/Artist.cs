using Hqub.Lastfm;
using MusicGQL.Features.LastFm;
using MusicGQL.Features.MusicBrainz.Artist;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Integration.MusicBrainz;
using TrackSeries.FanArtTV.Client;

namespace MusicGQL.Features.ServerLibrary;

public record Artist([property: GraphQLIgnore] ArtistJson Model)
{
    [ID]
    public string Id() => Model.Id;

    public string Name() => Model.Name;

    public string SortName() => Model.SortName;

    public ArtistServerStatus.ArtistServerStatus ServerStatus() => new(Model.Id);

    public async Task<IEnumerable<LastFmTrack>> TopTracks(LastfmClient lastfmClient)
    {
        var mbId = Model.Connections?.MusicBrainzArtistId;

        if (mbId is null)
        {
            return [];
        }

        try
        {
            var tracks = await lastfmClient.Artist.GetTopTracksByMbidAsync(Model.Id);

            return tracks.Take(20).Select(t => new LastFmTrack(t)).ToList();
        }
        catch
        {
            return [];
        }
    }

    public async Task<MbArtist?> MusicBrainzArtist(MusicBrainzService musicBrainzService)
    {
        // TODO Remove this, we should only use MB for imports.
        var mbId = Model.Connections?.MusicBrainzArtistId;

        if (mbId is null)
        {
            return null;
        }

        var artist = await musicBrainzService.GetArtistByIdAsync(mbId);

        if (artist == null)
        {
            return null;
        }

        return new(artist);
    }

    public async Task<IEnumerable<Release>> Releases(ServerLibraryCache cache)
    {
        var releases = await cache.GetAllReleasesAsync();
        return releases.Select(r => new Release(Model, r.ReleaseJson));
    }

    public async Task<long?> Listeners(LastfmClient lastfmClient)
    {
        var mbId = Model.Connections?.MusicBrainzArtistId;

        if (mbId is null)
        {
            return null;
        }

        try
        {
            var info = await lastfmClient.Artist.GetInfoByMbidAsync(mbId);
            return info.Statistics.Listeners;
        }
        catch
        {
            return null;
        }
    }

    public async Task<ArtistImages?> Images(IFanArtTVClient fanartClient)
    {
        var mbId = Model.Connections?.MusicBrainzArtistId;

        if (mbId is null)
        {
            return null;
        }

        try
        {
            var artist = await fanartClient.Music.GetArtistAsync(mbId);
            return artist is null ? null : new(artist);
        }
        catch
        {
            return null;
        }
    }
}
