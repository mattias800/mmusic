using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.ServerSettings.Events;

public class SoulSeekSearchTimeLimitUpdated : Event
{
    public int NewSeconds { get; set; }
}
