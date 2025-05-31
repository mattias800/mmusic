using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.ServerLibrary.Events;

public class AddArtistToServerLibrary : Event
{
    public required string ArtistId { get; set; }
}
