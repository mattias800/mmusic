using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.ServerSettings.Events;

public class DiscographySettingsUpdated : Event
{
    public bool Enabled { get; set; }
    public string? StagingPath { get; set; }
}


