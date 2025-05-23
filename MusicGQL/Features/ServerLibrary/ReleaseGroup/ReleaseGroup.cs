using MusicGQL.Features.MusicBrainz.ReleaseGroup;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Db;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup;

public record ReleaseGroup([property: GraphQLIgnore] DbReleaseGroup Model)
{
    public async Task<MbReleaseGroup?> MusicBrainzReleaseGroup(
        [Service] MusicBrainzService musicBrainzService
    )
    {
        var releaseGroup = await musicBrainzService.GetReleaseGroupByIdAsync(Model.Id);

        if (releaseGroup == null)
        {
            return null;
        }

        return new(releaseGroup);
    }
};
