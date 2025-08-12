using MusicGQL.Features.External;
using MusicGQL.Features.ServerLibrary.Json;

namespace MusicGQL.Features.Artists;

public record ArtistConnectedExternalService(
    [property: GraphQLIgnore] ExternalServiceModel Model,
    bool IsConnected,
    [property: GraphQLIgnore] JsonArtistServiceConnections? Connections = null
)
{
    public ExternalService ExternalService() => new(Model);

    public string ArtistPageUrl()
    {
        if (!IsConnected || Connections == null)
        {
            return string.Empty;
        }

        // Build per-service URL
        switch (Model.Id)
        {
            case "musicbrainz":
                return string.IsNullOrWhiteSpace(Connections.MusicBrainzArtistId)
                    ? string.Empty
                    : $"http://musicbrainz.org/artist/{Connections.MusicBrainzArtistId}";
            case "spotify":
                return string.IsNullOrWhiteSpace(Connections.SpotifyId)
                    ? string.Empty
                    : $"https://open.spotify.com/artist/{Connections.SpotifyId}";
            case "apple-music":
                return string.IsNullOrWhiteSpace(Connections.AppleMusicArtistId)
                    ? string.Empty
                    : $"https://music.apple.com/artist/{Connections.AppleMusicArtistId}";
            case "youtube":
                return Connections.YoutubeChannelUrl ?? string.Empty;
            case "tidal":
                return string.IsNullOrWhiteSpace(Connections.TidalArtistId)
                    ? string.Empty
                    : $"https://tidal.com/browse/artist/{Connections.TidalArtistId}";
            case "deezer":
                return string.IsNullOrWhiteSpace(Connections.DeezerArtistId)
                    ? string.Empty
                    : $"https://www.deezer.com/artist/{Connections.DeezerArtistId}";
            case "soundcloud":
                return Connections.SoundcloudUrl ?? string.Empty;
            case "bandcamp":
                return Connections.BandcampUrl ?? string.Empty;
            case "discogs":
                return Connections.DiscogsUrl ?? string.Empty;
            default:
                return string.Empty;
        }
    }
}