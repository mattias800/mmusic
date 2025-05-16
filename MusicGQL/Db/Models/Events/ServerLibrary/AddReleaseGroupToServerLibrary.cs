namespace MusicGQL.Db.Models.Events.ServerLibrary;

public class AddReleaseGroupToServerLibrary : Event
{
    public string ReleaseGroupMbId { get; set; }
}
