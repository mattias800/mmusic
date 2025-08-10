using SpotifyAPI.Web;

namespace MusicGQL.Features.Playlists.Import.Spotify;

public record SpotifyTrack([property: GraphQLIgnore] FullTrack Model)
{
    public string Id => Model.Id ?? string.Empty;
    public string Title => Model.Name ?? string.Empty;
    public int? DurationMs => Model.DurationMs;
    public string? PreviewUrl => Model.PreviewUrl;
    public IEnumerable<string> ArtistNames => Model.Artists?.Select(a => a.Name ?? string.Empty) ?? [];
    public string? AlbumCoverImageUrl => Model.Album?.Images?.FirstOrDefault()?.Url;
}


