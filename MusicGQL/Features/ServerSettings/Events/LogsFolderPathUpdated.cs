using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.ServerSettings.Events;

public class LogsFolderPathUpdated : Event
{
    public string? NewPath { get; set; }
}


