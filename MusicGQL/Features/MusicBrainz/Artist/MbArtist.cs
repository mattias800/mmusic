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

    // Artist server status removed

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

    public IEnumerable<Artists.ConnectedExternalService> ConnectedExternalServices()
    {
        // Build map of detected connections from relations
        var rels = Model.Relations ?? [];
        bool hasMb = !string.IsNullOrWhiteSpace(Model.Id);
        bool hasSpotify = false, hasApple = false, hasYoutube = false, hasTidal = false,
            hasDeezer = false, hasSoundcloud = false, hasBandcamp = false, hasDiscogs = false;

        foreach (var r in rels)
        {
            var url = r?.Url?.Resource;
            if (string.IsNullOrWhiteSpace(url)) continue;
            var u = url.ToLowerInvariant();
            if (!hasSpotify && u.Contains("open.spotify.com/artist/")) hasSpotify = true;
            else if (!hasApple && u.Contains("music.apple.com")) hasApple = true;
            else if (!hasYoutube && (u.Contains("youtube.com") || u.Contains("youtu.be"))) hasYoutube = true;
            else if (!hasTidal && u.Contains("tidal.com")) hasTidal = true;
            else if (!hasDeezer && u.Contains("deezer.com")) hasDeezer = true;
            else if (!hasSoundcloud && u.Contains("soundcloud.com")) hasSoundcloud = true;
            else if (!hasBandcamp && u.Contains("bandcamp.com")) hasBandcamp = true;
            else if (!hasDiscogs && u.Contains("discogs.com")) hasDiscogs = true;
        }

        yield return new Artists.ConnectedExternalService("musicbrainz", hasMb);
        yield return new Artists.ConnectedExternalService("spotify", hasSpotify);
        yield return new Artists.ConnectedExternalService("apple-music", hasApple);
        yield return new Artists.ConnectedExternalService("youtube", hasYoutube);
        yield return new Artists.ConnectedExternalService("tidal", hasTidal);
        yield return new Artists.ConnectedExternalService("deezer", hasDeezer);
        yield return new Artists.ConnectedExternalService("soundcloud", hasSoundcloud);
        yield return new Artists.ConnectedExternalService("bandcamp", hasBandcamp);
        yield return new Artists.ConnectedExternalService("discogs", hasDiscogs);
    }
}
