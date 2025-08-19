using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.ServerSettings.Events;

public class PublicBaseUrlUpdated : Event
{
    public string NewPublicBaseUrl { get; set; } = string.Empty;
}


