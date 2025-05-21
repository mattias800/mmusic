using Hqub.Lastfm;
using MusicGQL.Features.LastFm;
using MusicGQL.Features.Recording;
using MusicGQL.Features.ReleaseGroups;
using MusicGQL.Features.ServerLibrary.Artist;
using MusicGQL.Integration.MusicBrainz;
using TrackSeries.FanArtTV.Client;

namespace MusicGQL.Features.Artist;

public record Artist([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Artist Model)
{
    [ID]
    public string Id => Model.Id;
    public string Name => Model.Name;
    public string SortName => Model.SortName;
    public string? Disambiguation => Model.Disambiguation;
    public string? Type => Model.Type;

    public async Task<IEnumerable<Release.Release>> Releases([Service] MusicBrainzService mbService)
    {
        var releases = await mbService.GetReleasesForArtistAsync(Id);
        return releases.Select(r => new Release.Release(r));
    }

    public async Task<IEnumerable<ReleaseGroup>> ReleaseGroups(
        [Service] MusicBrainzService mbService
    )
    {
        var releaseGroups = await mbService.GetReleaseGroupsForArtistAsync(Id);
        return releaseGroups.Select(r => new ReleaseGroup(r));
    }

    public async Task<IEnumerable<ReleaseGroup>> Albums([Service] MusicBrainzService mbService)
    {
        var releaseGroups = await mbService.GetReleaseGroupsForArtistAsync(Id);
        var albumReleaseGroups = releaseGroups.Where(r => r.IsMainAlbum()).ToList();

        return albumReleaseGroups.Select(r => new ReleaseGroup(r));
    }

    public async Task<IEnumerable<ReleaseGroup>> Singles([Service] MusicBrainzService mbService)
    {
        var releaseGroups = await mbService.GetReleaseGroupsForArtistAsync(Id);
        var albumReleaseGroups = releaseGroups.Where(r => r.IsMainSingle()).ToList();

        return albumReleaseGroups.Select(r => new ReleaseGroup(r));
    }

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

    public async Task<ArtistImages?> Images([Service] IFanArtTVClient fanartClient)
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

    public async Task<long?> Listeners([Service] LastfmClient lastfmClient)
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

    public ArtistServerAvailability ServerAvailability() => new(Model.Id);
}
