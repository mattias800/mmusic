using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.ServerLibrary.Artist.Db;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Integration.Neo4j;

namespace MusicGQL.Features.ServerLibrary.Artist;

public record ArtistSearchRoot
{
    public async Task<IEnumerable<Artist>> All(
        ServerLibraryService service,
        EventDbContext dbContext,
        [Service] IHttpContextAccessor httpContextAccessor
    )
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );
        if (!Guid.TryParse(userIdString, out var userId))
        {
            // Handle error: User not found or not authenticated
            return []; // Or throw an exception
        }

        var addedArtistMbIds = await dbContext
            .ServerArtists.Where(sa => sa.AddedByUserId == userId)
            .Select(sa => sa.ArtistId)
            .Distinct()
            .ToListAsync();

        if (!addedArtistMbIds.Any())
        {
            return [];
        }

        var allArtists = await service.GetArtistsByIdsAsync(addedArtistMbIds);

        return allArtists.Select(a => new Artist(a));
    }

    public async Task<Artist?> ById(
        ServerLibraryService service,
        MusicBrainzService mbService,
        [ID] string id
    )
    {
        var artist = await service.GetArtistByIdAsync(id);
        if (artist is null)
        {
            var a = await mbService.GetArtistByIdAsync(id);
            if (a is null)
            {
                return null;
            }

            return new(
                new DbArtist
                {
                    Id = a.Id,
                    Name = a.Name,
                    SortName = a.SortName,
                    Gender = a.Gender,
                }
            );
        }

        return new(artist);
    }

    public async Task<IEnumerable<Artist>> SearchByName(
        ServerLibraryService service,
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var artists = await service.SearchArtistByNameAsync(name, limit, offset);
        return artists.Select(a => new Artist(a));
    }
}
