using MusicGQL.Db;
using MusicGQL.Db.Postgres;

namespace MusicGQL.Features.ServerLibrary.Artist;

public record ArtistsInServerLibrarySearchRoot
{
    public async Task<IEnumerable<ArtistInServerLibrary>> All(EventDbContext dbContext)
    {
        var p = await dbContext.ArtistsAddedToServerLibraryProjection.FindAsync(1);
        return p?.ArtistMbIds.Select(artistMbId => new ArtistInServerLibrary(artistMbId)) ?? [];
    }
};
