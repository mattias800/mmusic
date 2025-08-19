using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.ServerSettings.Events;

public class SoulSeekConnectionUpdated : Event
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
}


