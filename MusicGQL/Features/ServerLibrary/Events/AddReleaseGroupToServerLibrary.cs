using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.ServerLibrary.Events;

public class AddReleaseGroupToServerLibrary : Event
{
    public required string ReleaseGroupId { get; set; }
}
