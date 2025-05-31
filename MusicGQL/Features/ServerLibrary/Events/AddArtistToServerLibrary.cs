using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.ServerLibrary.Events;

public class AddArtistToServerLibrary : Event
{
    public string ArtistMbId { get; set; }
}
