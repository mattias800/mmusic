using Hqub.Lastfm.Entities;
using MusicGQL.Features.MusicBrainz.Recording;
using MusicGQL.Features.ServerLibrary.Recording;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Integration.Neo4j;

namespace MusicGQL.Features.LastFm;

public record LastFmTrack([property: GraphQLIgnore] Track Model)
{
    [ID]
    public string Id => Model.Url;
    public string MBID => Model.MBID;
    public string Name => Model.Name;
    public LastFmArtist Artist => new(Model.Artist);
    public LastFmAlbum Album => new(Model.Album);

    public long? PlayCount => Model.Statistics.PlayCount;
    public string? Summary => Model.Wiki?.Summary;

    public LastFmStatistics Statistics => new(Model.Statistics);

    public async Task<Recording?> Recording(ServerLibraryImportService service)
    {
        var r = await service.SearchRecordingForArtistByArtistNameAsync(
            Model.Name,
            Model.Artist.Name
        );

        var f = r.FirstOrDefault();

        return f is null ? null : new Recording(f);
    }

    public async Task<MbRecording?> MusicBrainzRecording(MusicBrainzService service)
    {
        if (string.IsNullOrEmpty(Model.MBID))
        {
            return null;
        }

        var release = await service.GetRecordingByIdAsync(Model.MBID);
        return release is null ? null : new MbRecording(release);
    }
}
