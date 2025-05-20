using SpotifyAPI.Web;

namespace MusicGQL.Features.Playlists.Import.Spotify;

public record SpotifyPlaylist([property: GraphQLIgnore] FullPlaylist Model)
{
    public string Id => Model.Id ?? "";
    public string Name => Model.Name ?? "";
    public string? Description => Model.Description;
    public string? CoverImageUrl => Model.Images?.FirstOrDefault()?.Url;
}
