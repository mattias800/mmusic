using SpotifyAPI.Web;
using MusicGQL.Integration.Spotify;

namespace MusicGQL.Features.Playlists.Import.Spotify;

public record SpotifyPlaylist([property: GraphQLIgnore] FullPlaylist Model)
{
    public string Id => Model.Id ?? "";
    public string Name => Model.Name ?? "";
    public string? Description => Model.Description;
    public string? CoverImageUrl => Model.Images?.FirstOrDefault()?.Url;

    public async Task<IEnumerable<SpotifyTrack>> Tracks([Service] SpotifyService spotifyService)
    {
        var tracks = await spotifyService.GetTracksFromPlaylist(Id) ?? [];
        return tracks.Select(t => new SpotifyTrack(t));
    }

    public int? TotalTracks() => Model.Tracks?.Total;
}
