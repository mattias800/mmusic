using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.ServerLibrary.Events;

public class AddReleaseGroupToServerLibrary : Event
{
    public string ReleaseGroupMbId { get; set; }
}
