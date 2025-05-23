using MusicGQL.Db.Postgres;
using MusicGQL.Features.MusicBrainz.Artist;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.ServerLibrary.Artist;

public record ArtistServerAvailability([property: GraphQLIgnore] string ArtistMbId)
{
    [ID]
    public string Id() => ArtistMbId;

    public async Task<MbArtist?> GetArtist([Service] MusicBrainzService musicBrainzService)
    {
        var artist = await musicBrainzService.GetArtistByIdAsync(ArtistMbId);

        if (artist == null)
        {
            return null;
        }

        return new(artist);
    }

    public async Task<bool> IsInServerLibrary([Service] EventDbContext dbContext)
    {
        var artist = await dbContext.ArtistsAddedToServerLibraryProjection.FindAsync(1);
        return artist?.ArtistMbIds.Contains(ArtistMbId) ?? false;
    }
}
