using System.Collections;
using Hqub.Lastfm;
using MusicGQL.Features.Recording;
using MusicGQL.Features.Release;
using MusicGQL.Integration.MusicBrainz;
using TrackSeries.FanArtTV.Client;
using Track = Hqub.Lastfm.Entities.Track;

namespace MusicGQL.Features.Artist;

public record Artist([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Artist Model)
{
    [ID] public string Id => Model.Id;
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

    public async Task<IEnumerable<Release.Release>> MainAlbums([Service] MusicBrainzService mbService)
    {
        var releaseGroups = await mbService.GetReleaseGroupsForArtistAsync(Id);
        var albumReleaseGroups = releaseGroups.Where(r => r.IsMainAlbum()).ToList();
        var albums = await Task.WhenAll(
            albumReleaseGroups.Select(async rg =>
            {
                var releases = await mbService.GetReleasesForReleaseGroupAsync(rg.Id);
                return MainAlbumFinder.GetMainReleaseInReleaseGroup(releases);
            })
        );

        return albums
            .OfType<Hqub.MusicBrainz.Entities.Release>()
            .Where(r => r.Status == "Official")
            .Select(r => new Release.Release(r));
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
                .Select(t => new LastFmTrack(t)).ToList();
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
            return new(artist);
        }
        catch
        {
            return null;
        }
    }
}