using Rebus.Sagas;

namespace MusicGQL.Features.ServerLibrary.Artist.Sagas;

public class AddArtistToServerLibrarySagaData : ISagaData
{
    public Guid Id { get; set; }
    public int Revision { get; set; }

    public string ArtistMbId { get; set; } = string.Empty;

    // UI fields
    public string StatusDescription { get; set; } = string.Empty;
}
