using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.ServerSettings.Events;

public class QBittorrentSettingsUpdated : Event
{
    public string? BaseUrl { get; set; }
    public string? Username { get; set; }
    public string? SavePath { get; set; }
}
