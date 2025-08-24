namespace MusicGQL.Features.Spotify;

public record SpotifyArtist
{
    public SpotifyArtist(SpotifyAPI.Web.FullArtist artist)
    {
        Id = artist.Id ?? string.Empty;
        Name = artist.Name ?? string.Empty;
        Images =
            artist.Images?.Select(i => new SpotifyImage { Url = i.Url }).ToList()
            ?? new List<SpotifyImage>();
    }

    public string Id { get; init; }
    public string Name { get; init; }
    public List<SpotifyImage> Images { get; init; }
}
