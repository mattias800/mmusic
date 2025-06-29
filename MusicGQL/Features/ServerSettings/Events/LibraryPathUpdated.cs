using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.ServerSettings.Events;

public class LibraryPathUpdated : Event
{
    public string NewLibraryPath { get; set; }
}
