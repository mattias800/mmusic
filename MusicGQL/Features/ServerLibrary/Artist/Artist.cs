using Hqub.Lastfm;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.LastFm;
using MusicGQL.Features.MusicBrainz.Artist;
using MusicGQL.Features.ServerLibrary.Artist.Db;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Integration.Neo4j;
using TrackSeries.FanArtTV.Client;

namespace MusicGQL.Features.ServerLibrary.Artist;

public record Artist([property: GraphQLIgnore] DbArtist Model)
{
    [ID]
    public string Id => Model.Id;
    public string Name => Model.Name;
    public string SortName => Model.SortName;
    public string? Gender => Model.Gender;

    public ArtistServerAvailability ServerAvailability() => new(Model.Id);

    public async Task<IEnumerable<LastFmTrack>> TopTracks(LastfmClient lastfmClient)
    {
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
        var artist = await musicBrainzService.GetArtistByIdAsync(Model.Id);

        if (artist == null)
        {
            return null;
        }

        return new(artist);
    }

    public async Task<IEnumerable<Release.Release>> Releases(Neo4jService service)
    {
        var releases = await service.GetReleasesForArtistAsync(Model.Id);
        return releases.Select(r => new Release.Release(r));
    }

    public async Task<IEnumerable<ReleaseGroup.ReleaseGroup>> ReleaseGroups(Neo4jService service)
    {
        var releaseGroups = await service.GetReleaseGroupsForArtistAsync(Model.Id);
        return releaseGroups.Select(r => new ReleaseGroup.ReleaseGroup(r));
    }

    public async Task<long?> Listeners(LastfmClient lastfmClient)
    {
        try
        {
            var info = await lastfmClient.Artist.GetInfoByMbidAsync(Model.Id);
            return info.Statistics.Listeners;
        }
        catch
        {
            return null;
        }
    }

    public async Task<MbArtistImages?> Images(IFanArtTVClient fanartClient)
    {
        try
        {
            var artist = await fanartClient.Music.GetArtistAsync(Model.Id);
            return artist is null ? null : new(artist);
        }
        catch
        {
            return null;
        }
    }

    public async Task<IEnumerable<ReleaseGroup.ReleaseGroup>> Albums(Neo4jService service)
    {
        var releaseGroups = await service.GetReleaseGroupsForArtistAsync(Model.Id);
        var albumReleaseGroups = releaseGroups.Where(r => r.IsMainAlbum()).ToList();

        return albumReleaseGroups.Select(r => new ReleaseGroup.ReleaseGroup(r));
    }

    public async Task<IEnumerable<ReleaseGroup.ReleaseGroup>> Singles(Neo4jService service)
    {
        var releaseGroups = await service.GetReleaseGroupsForArtistAsync(Model.Id);
        var albumReleaseGroups = releaseGroups.Where(r => r.IsMainSingle()).ToList();

        return albumReleaseGroups.Select(r => new ReleaseGroup.ReleaseGroup(r));
    }

    public async Task<IEnumerable<ReleaseGroup.ReleaseGroup>> Eps(Neo4jService service)
    {
        var releaseGroups = await service.GetReleaseGroupsForArtistAsync(Model.Id);
        var albumReleaseGroups = releaseGroups.Where(r => r.IsMainEP()).ToList();

        return albumReleaseGroups.Select(r => new ReleaseGroup.ReleaseGroup(r));
    }
}
