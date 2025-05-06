using Hqub.Lastfm.Entities;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.Recording;

public record LastFmTrack([property: GraphQLIgnore] Track Model)
{
    [ID]
    public string Id => Model.MBID;

    public int? UserPlayCount => Model.UserPlayCount;
    public long? PlayCount => Model.Statistics.PlayCount;
    public string? Summary => Model.Wiki?.Summary;

    public async Task<Recording?> Recording([Service] MusicBrainzService mbService)
    {
        var release = await mbService.GetRecordingByIdAsync(Model.MBID);
        return release is null ? null : new Recording(release);
    }
}
