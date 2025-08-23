using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.ServerSettings.Events;

public class DownloadSlotCountUpdated : Event
{
    public int NewSlotCount { get; set; }
}

public class SoulSeekMaxReleasesPerUserDiscoveryUpdated : Event
{
    public int NewMaxReleases { get; set; }
}
