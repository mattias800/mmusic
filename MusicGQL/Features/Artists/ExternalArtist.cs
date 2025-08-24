using MusicGQL.Features.Playlists.Events;

namespace MusicGQL.Features.Artists;

public record ExternalArtist(string ArtistId, string ArtistName, ExternalServiceType Service)
{
    [ID]
    public string Id() => Service + ":" + ArtistId;
}
