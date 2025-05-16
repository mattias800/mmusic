using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup;

public record ReleaseGroupInServerLibrary([property: GraphQLIgnore] string ReleaseGroupMbId)
{
    public async Task<ReleaseGroups.ReleaseGroup?> GetReleaseGroup(
        [Service] MusicBrainzService musicBrainzService
    )
    {
        var releaseGroup = await musicBrainzService.GetReleaseGroupByIdAsync(ReleaseGroupMbId);

        if (releaseGroup == null)
        {
            return null;
        }

        return new(releaseGroup);
    }
};
