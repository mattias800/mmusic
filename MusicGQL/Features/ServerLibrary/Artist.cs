using Hqub.Lastfm;
using MusicGQL.Features.LastFm;
using MusicGQL.Features.ServerLibrary.Cache;
using TrackSeries.FanArtTV.Client;

namespace MusicGQL.Features.ServerLibrary;

public record Artist([property: GraphQLIgnore] CachedArtist Model)
{
    [ID]
    public string Id() => Model.Id;

    public string Name() => Model.Name;

    public string SortName() => Model.SortName ?? Model.Name;

    public ArtistServerStatus.ArtistServerStatus ServerStatus() => new(Model.Id);

    public async Task<IEnumerable<LastFmTrack>> TopTracks(LastfmClient lastfmClient)
    {
        var mbId = Model.ArtistJson.Connections?.MusicBrainzArtistId;

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

    public async Task<IEnumerable<Release>> Releases(ServerLibraryCache cache)
    {
        var releases = await cache.GetAllReleasesForArtistAsync(Model.Id);
        return releases.Select(r => new Release(r));
    }

    public async Task<long?> Listeners(LastfmClient lastfmClient)
    {
        var mbId = Model.ArtistJson.Connections?.MusicBrainzArtistId;

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

    public ArtistImages? Images()
    {
        if (Model.ArtistJson.Photos == null)
        {
            return null;
        }

        return new ArtistImages(Model.ArtistJson.Photos, Model.Id);
    }
}
