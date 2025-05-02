using Hqub.MusicBrainz;

namespace MusicGQL.Features.Artist;

public record ArtistSearchRoot
{
    public async Task<IEnumerable<Artist>> SearchByName([Service] MusicBrainzClient client, string name)
    {
        var artists = await client.Artists.SearchAsync(name, 20);
        return artists.Items.Select(a => new Artist(a));
    }

    public async Task<Artist?> ById([Service] MusicBrainzClient client, string id)
    {
        var artist = await client.Artists.GetAsync(id);
        return artist != null ? new Artist(artist) : null;
    }
}