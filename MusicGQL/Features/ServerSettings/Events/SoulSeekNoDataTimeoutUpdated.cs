using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.ServerSettings.Events;

public class SoulSeekNoDataTimeoutUpdated : Event
{
    public int NewSeconds { get; set; }
}
