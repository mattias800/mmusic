using Hqub.Lastfm;
using MusicGQL.Features.LastFm;
using MusicGQL.Features.MusicBrainz.ReleaseGroup;
using MusicGQL.Features.ServerLibrary.Utils;
using MusicGQL.Integration.MusicBrainz;
using TrackSeries.FanArtTV.Client;
using MusicGQL.Features.Artists;

namespace MusicGQL.Features.MusicBrainz.Artist;

public record MbArtist([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Artist Model) : IArtistBase
{
    [ID]
    public string Id() => Model.Id;

    public string Name() => Model.Name;

    public string SortName() => Model.SortName;

    public string? Disambiguation() => Model.Disambiguation;
    public string? Type() => Model.Type;

    public string? Country() => Model.Country;

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

    public async Task<MbArtistImages?> Images(IFanArtTVClient fanartClient)
    {
        try
        {
            var artistImages = await fanartClient.Music.GetArtistAsync(Model.Id);
            return artistImages is null ? null : new(artistImages);
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

    public IEnumerable<Common.MbRelation> Relations()
    {
        return Model.Relations?.Select(r => new Common.MbRelation(r)) ?? [];
    }

    public async Task<LastFmArtist?> LastFmArtist(LastfmClient lastfmClient)
    {
        try
        {
            var artist = await lastfmClient.Artist.GetInfoByMbidAsync(Model.Id);
            return artist is null ? null : new LastFmArtist(artist);
        }
        catch
        {
            return null;
        }
    }
}