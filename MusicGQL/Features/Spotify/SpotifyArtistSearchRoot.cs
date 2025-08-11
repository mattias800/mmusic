using MusicGQL.Integration.Spotify;

namespace MusicGQL.Features.Spotify;

public record SpotifyArtistSearchRoot
{
    public async Task<IEnumerable<SpotifyArtist>> SearchByName(
        [Service] SpotifyService spotifyService,
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var results = await spotifyService.SearchArtistsAsync(name, limit + offset);
        var paged = results?.Skip(offset).Take(limit) ?? Enumerable.Empty<SpotifyAPI.Web.FullArtist>();
        return paged.Select(a => new SpotifyArtist(a));
    }

    public async Task<SpotifyArtist?> ById([Service] SpotifyService spotifyService, [ID] string id)
    {
        var artist = await spotifyService.GetArtistAsync(id);
        return artist != null ? new SpotifyArtist(artist) : null;
    }
}