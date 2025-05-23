using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.MusicBrainz.Artist;

public record MusicBrainzArtistSearchRoot
{
    public async Task<IEnumerable<MbArtist>> SearchByName(
        [Service] MusicBrainzService mbService,
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var artists = await mbService.SearchArtistByNameAsync(name, limit, offset);
        return artists.Select(a => new MbArtist(a));
    }

    public async Task<MbArtist?> ById([Service] MusicBrainzService mbService, [ID] string id)
    {
        var artist = await mbService.GetArtistByIdAsync(id);
        return artist != null ? new MbArtist(artist) : null;
    }
}
