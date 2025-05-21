using Hqub.Lastfm.Entities;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.LastFm;

public record LastFmTrack([property: GraphQLIgnore] Track Model)
{
    [ID]
    public string Id => Model.MBID;
    public string Name => Model.Name;
    public LastFmArtist Artist => new(Model.Artist);
    public LastFmAlbum Album => new(Model.Album);

    public long? PlayCount => Model.Statistics.PlayCount;
    public string? Summary => Model.Wiki?.Summary;

    public LastFmStatistics Statistics => new(Model.Statistics);

    public async Task<Recording.Recording?> Recording([Service] MusicBrainzService mbService)
    {
        if (string.IsNullOrEmpty(Model.MBID))
        {
            return null;
        }

        var release = await mbService.GetRecordingByIdAsync(Model.MBID);
        return release is null ? null : new Recording.Recording(release);
    }
}
