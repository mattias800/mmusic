using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.ServerSettings.Events;

public class DownloadPathUpdated : Event
{
    public string NewPath { get; set; }
}
