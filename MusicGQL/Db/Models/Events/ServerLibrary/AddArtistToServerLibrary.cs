namespace MusicGQL.Db.Models.Events.ServerLibrary;

public class AddArtistToServerLibrary : Event
{
    public required string ArtistMbId { get; set; }
}
