using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.ServerLibrary.Artist;

public record ArtistInServerLibrary([property: GraphQLIgnore] string ArtistMbId)
{
    public async Task<Features.Artist.Artist?> GetArtist(
        [Service] MusicBrainzService musicBrainzService
    )
    {
        var artist = await musicBrainzService.GetArtistByIdAsync(ArtistMbId);

        if (artist == null)
        {
            return null;
        }

        return new(artist);
    }
}
