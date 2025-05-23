using Hqub.Lastfm;
using MusicGQL.Common;
using MusicGQL.Features.LastFm;
using MusicGQL.Features.MusicBrainz.Artist;
using MusicGQL.Features.MusicBrainz.Common;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.MusicBrainz.Recording;

public record MbRecording([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Recording Model)
{
    [ID]
    public string Id => Model.Id;
    public string Title => Model.Title;
    public int? Length => Model.Length;

    public IEnumerable<NameCredit> NameCredits =>
        Model.Credits?.Select(c => new NameCredit(c)) ?? [];

    public async Task<IEnumerable<Release.MbRelease>> Releases(
        [Service] MusicBrainzService mbService
    )
    {
        var releases = await mbService.GetReleasesForRecordingAsync(Model.Id);
        return releases.Select(a => new Release.MbRelease(a));
    }

    public async Task<Release.MbRelease?> MainAlbum([Service] MusicBrainzService mbService)
    {
        var releases = await mbService.GetReleasesForRecordingAsync(Model.Id);
        var mainAlbum = MainAlbumFinder.FindMainAlbumForSong(releases);
        return mainAlbum is null ? null : new Release.MbRelease(mainAlbum);
    }

    public async Task<IEnumerable<MbArtist>> Artists([Service] MusicBrainzService mbService)
    {
        var artists = await mbService.GetArtistsForRecordingAsync(Model.Id);
        return artists.Select(a => new MbArtist(a));
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
