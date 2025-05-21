using Hqub.Lastfm;
using MusicGQL.Common;
using MusicGQL.Features.LastFm;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.Recording;

public record Recording([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Recording Model)
{
    [ID]
    public string Id => Model.Id;
    public string Title => Model.Title;
    public int? Length => Model.Length;

    public IEnumerable<NameCredit.NameCredit> NameCredits =>
        Model.Credits?.Select(c => new NameCredit.NameCredit(c)) ?? [];

    public async Task<IEnumerable<Release.Release>> Releases([Service] MusicBrainzService mbService)
    {
        var releases = await mbService.GetReleasesForRecordingAsync(Model.Id);
        return releases.Select(a => new Release.Release(a));
    }

    public async Task<Release.Release?> MainAlbum([Service] MusicBrainzService mbService)
    {
        var releases = await mbService.GetReleasesForRecordingAsync(Model.Id);
        var mainAlbum = MainAlbumFinder.FindMainAlbumForSong(releases);
        return mainAlbum is null ? null : new Release.Release(mainAlbum);
    }

    public async Task<IEnumerable<Artist.Artist>> Artists([Service] MusicBrainzService mbService)
    {
        var artists = await mbService.GetArtistsForRecordingAsync(Model.Id);
        return artists.Select(a => new Artist.Artist(a));
    }

    public IEnumerable<Relation> Relations()
    {
        return Model.Relations?.Select(r => new Relation(r)) ?? [];
    }

    public async Task<LastFmStatistics?> Statistics([Service] LastfmClient lastfmClient)
    {
        try
        {
            var track = await lastfmClient.Track.GetInfoByMbidAsync(Model.Id);
            return track is null ? null : new(track.Statistics);
        }
        catch
        {
            return null;
        }
    }

    public RecordingStreamingServiceInfo StreamingServiceInfo() => new(Model);
}
