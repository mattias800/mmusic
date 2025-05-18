namespace MusicGQL.Db.Postgres.Models.Events.ServerLibrary;

public class AddReleaseGroupToServerLibrary : Event
{
    public string ReleaseGroupMbId { get; set; }
}
