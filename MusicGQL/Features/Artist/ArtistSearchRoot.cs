using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.Artist;

public record ArtistSearchRoot
{
    public async Task<IEnumerable<Artist>> SearchByName(
        [Service] MusicBrainzService mbService,
        string name
    )
    {
        var artists = await mbService.SearchArtistByNameAsync(name);
        return artists.Select(a => new Artist(a));
    }

    public async Task<Artist?> ById([Service] MusicBrainzService mbService, [ID] string id)
    {
        var artist = await mbService.GetArtistByIdAsync(id);
        return artist != null ? new Artist(artist) : null;
    }
}
