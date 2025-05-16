namespace MusicGQL.Db.Models.Events.ServerLibrary;

public class AddReleaseGroupToServerLibrary : Event
{
    public required string ReleaseGroupMbId { get; set; }
}
