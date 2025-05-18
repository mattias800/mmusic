using Rebus.Sagas;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup.Sagas;

public class AddReleaseGroupToServerLibrarySagaData : ISagaData
{
    public Guid Id { get; set; }
    public int Revision { get; set; }

    public string ReleaseGroupMbId { get; set; } = string.Empty;

    // UI fields
    public string StatusDescription { get; set; } = string.Empty;
}
