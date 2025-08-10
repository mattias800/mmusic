namespace MusicGQL.Features.Playlists.Import.Spotify;

public record SpotifyPlaylistModel
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? CoverImageUrl { get; init; }
    public int? TotalTracks { get; init; }
}


