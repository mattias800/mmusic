namespace MusicGQL.Features.ServerLibrary.ReleaseGroup.Sagas;

public record AddReleaseGroupToServerLibrarySagaEvents
{
    public record StartAddReleaseGroup(string ReleaseGroupMbId);

    public record FindReleaseGroupInMusicBrainz(string ReleaseGroupMbId);

    public record FoundReleaseGroupInMusicBrainz(
        string ReleaseGroupMbId,
        Hqub.MusicBrainz.Entities.ReleaseGroup ReleaseGroup
    );

    public record DidNotFindReleaseGroupInMusicBrainz(string ReleaseGroupMbId);
}
