namespace MusicGQL.Db.Postgres.Models.Events.ServerLibrary;

public class AddArtistToServerLibrary : Event
{
    public string ArtistMbId { get; set; }
}
