using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.ServerSettings.Events;

public class ProwlarrSettingsUpdated : Event
{
    public string? BaseUrl { get; set; }
    public int TimeoutSeconds { get; set; }
    public int MaxRetries { get; set; }
    public int RetryDelaySeconds { get; set; }
    public bool TestConnectivityFirst { get; set; }
    public bool EnableDetailedLogging { get; set; }
    public int MaxConcurrentRequests { get; set; }
}


