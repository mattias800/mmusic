using Hqub.Lastfm;
using MusicGQL.Features.LastFm;
using MusicGQL.Features.MusicBrainz.ReleaseGroup;
using MusicGQL.Features.ServerLibrary;
using MusicGQL.Integration.MusicBrainz;
using TrackSeries.FanArtTV.Client;

namespace MusicGQL.Features.MusicBrainz.Artist;

public record MbArtist([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Artist Model)
{
    [ID]
    public string Id() => Model.Id;

    public string Name() => Model.Name;

    public string SortName() => Model.SortName;

    public string? Disambiguation => Model.Disambiguation;
    public string? Type => Model.Type;

    public async Task<IEnumerable<Release.MbRelease>> Releases(MusicBrainzService mbService)
    {
        var releases = await mbService.GetReleasesForArtistAsync(Model.Id);
        return releases.Select(r => new Release.MbRelease(r));
    }

    public async Task<IEnumerable<MbReleaseGroup>> ReleaseGroups(MusicBrainzService mbService)
    {
        var releaseGroups = await mbService.GetReleaseGroupsForArtistAsync(Model.Id);
        return releaseGroups.Select(r => new MbReleaseGroup(r));
    }

    public async Task<IEnumerable<MbReleaseGroup>> Albums(MusicBrainzService mbService)
    {
        var releaseGroups = await mbService.GetReleaseGroupsForArtistAsync(Model.Id);
        var albumReleaseGroups = releaseGroups.Where(r => r.IsMainAlbum()).ToList();

        return albumReleaseGroups.Select(r => new MbReleaseGroup(r));
    }

    public async Task<IEnumerable<MbReleaseGroup>> Singles(MusicBrainzService mbService)
    {
        var releaseGroups = await mbService.GetReleaseGroupsForArtistAsync(Model.Id);
        var albumReleaseGroups = releaseGroups.Where(r => r.IsMainSingle()).ToList();

        return albumReleaseGroups.Select(r => new MbReleaseGroup(r));
    }

    public async Task<IEnumerable<LastFmTrack>> TopTracks(LastfmClient lastfmClient)
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

    public async Task<ArtistImages?> Images(IFanArtTVClient fanartClient)
    {
        try
        {
            // var artist = await fanartClient.Music.GetArtistAsync(Model.Id);
            // return artist is null ? null : new(artist);
            return null;
        }
        catch
        {
            return null;
        }
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

    public ArtistServerStatus.ArtistServerStatus ServerStatus() => new(Model.Id);
}
