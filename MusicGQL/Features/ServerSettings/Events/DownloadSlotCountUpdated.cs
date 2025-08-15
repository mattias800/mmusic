using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.ServerSettings.Events;

public class DownloadSlotCountUpdated : Event
{
    public int NewSlotCount { get; set; }
}
