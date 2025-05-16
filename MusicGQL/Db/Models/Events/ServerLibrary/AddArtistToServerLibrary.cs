namespace MusicGQL.Db.Models.Events.ServerLibrary;

public class AddArtistToServerLibrary : Event
{
    public string ArtistMbId { get; set; }
}
