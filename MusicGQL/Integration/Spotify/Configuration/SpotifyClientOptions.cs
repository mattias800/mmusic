namespace MusicGQL.Features.Spotify.Configuration;

public class SpotifyClientOptions
{
    public const string SectionName = "Spotify";

    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
} 