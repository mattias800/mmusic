using MusicGQL.Features.MusicBrainz.ReleaseGroup;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.LastFm;

public record LastFmAlbum([property: GraphQLIgnore] Hqub.Lastfm.Entities.Album Model)
{
    [ID]
    public string Id => Model.MBID;

    public string Name => Model.Name;

    public LastFmStatistics Statistics => new(Model.Statistics);

    public async Task<MbReleaseGroup?> Album([Service] MusicBrainzService mbService)
    {
        if (string.IsNullOrEmpty(Model.MBID))
        {
            return null;
        }

        var releaseGroup = await mbService.GetReleaseGroupByIdAsync(Model.MBID);
        return releaseGroup is null ? null : new MbReleaseGroup(releaseGroup);
    }
}
