using Hqub.Lastfm;
using MusicGQL.Features.LastFm;
using MusicGQL.Features.MusicBrainz.Artist;
using MusicGQL.Features.MusicBrainz.Common;
using MusicGQL.Features.ServerLibrary;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.MusicBrainz.Recording;

public record MbRecording([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Recording Model)
{
    [ID]
    public string Id => Model.Id;
    public string Title => Model.Title;
    public int? Length => Model.Length;

    public IEnumerable<MbNameCredit> NameCredits =>
        Model.Credits?.Select(c => new MbNameCredit(c)) ?? [];

    public async Task<IEnumerable<Release.MbRelease>> Releases(MusicBrainzService mbService)
    {
        var releases = await mbService.GetReleasesForRecordingAsync(Model.Id);
        return releases.Select(a => new Release.MbRelease(a));
    }

    // TODO Should return release group.
    public async Task<Release.MbRelease?> MainAlbum(MusicBrainzService mbService)
    {
        var releases = await mbService.GetReleasesForRecordingAsync(Model.Id);
        var mainAlbum = LibraryDecider.FindMainAlbumForSong(releases);
        return mainAlbum is null ? null : new Release.MbRelease(mainAlbum);
    }

    public async Task<IEnumerable<MbArtist>> Artists(MusicBrainzService service)
    {
        var artists = await service.GetArtistsForRecordingAsync(Model.Id);
        return artists.Select(a => new MbArtist(a));
    }

    public IEnumerable<MbRelation> Relations()
    {
        return Model.Relations?.Select(r => new MbRelation(r)) ?? [];
    }

    public async Task<LastFmStatistics?> Statistics(LastfmClient lastfmClient)
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

    public MbRecordingStreamingServiceInfo StreamingServiceInfo() => new(Model);
}
