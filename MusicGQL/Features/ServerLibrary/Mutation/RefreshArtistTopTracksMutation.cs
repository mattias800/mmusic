using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Artists;
using MusicGQL.Features.Import.Services;
using MusicGQL.Features.Playlists.Commands;
using MusicGQL.Features.ServerLibrary.Cache;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class RefreshArtistTopTracksMutation
{
    [Authorize(Policy = "IsAuthenticatedUser")]
    public async Task<RefreshArtistTopTracksResult> RefreshArtistTopTracks(
        ServerLibraryCache cache,
        LastFmEnrichmentService enrichment,
        ClaimsPrincipal claimsPrincipal,
        RenamePlaylistHandler renamePlaylistHandler,
        EventDbContext dbContext,
        RefreshArtistTopTracksInput input
    )
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        var userId = Guid.Parse(userIdClaim!.Value);

        var user = await dbContext.Users.FirstOrDefaultAsync(up => up.UserId == userId);
        if (user is null)
        {
            throw new GraphQLException(
                ErrorBuilder
                    .New()
                    .SetMessage("Authenticated user not found after authorization policy success.")
                    .SetCode("INTERNAL_SERVER_ERROR")
                    .Build()
            );
        }

        var artist = await cache.GetArtistByIdAsync(input.ArtistId);
        var mbId = artist?.JsonArtist.Connections?.MusicBrainzArtistId;
        if (artist == null || string.IsNullOrWhiteSpace(mbId))
        {
            return new RefreshArtistTopTracksUnknownError("Could not find artist in library");
        }

        var dir = Path.Combine("./Library/", input.ArtistId);

        // Clean old top tracks photos (toptrack*.jpg) in artist folder
        try
        {
            var files = Directory.GetFiles(dir, "toptrack*.jpg");
            foreach (var f in files)
            {
                File.Delete(f);
            }
        }
        catch { }

        // Re-enrich top tracks (fetch, complete, remap); enrichment overwrites artist.json
        await enrichment.EnrichArtistAsync(dir, mbId!);

        // Reload cache
        await cache.UpdateCacheAsync();

        // Return updated artist node
        var updated = await cache.GetArtistByIdAsync(input.ArtistId);
        return updated == null
            ? new RefreshArtistTopTracksUnknownError("Could not find artist in library")
            : new RefreshArtistTopTracksSuccess(new Artist(updated));
    }
}

public record RefreshArtistTopTracksInput([ID] string ArtistId);

[UnionType("RefreshArtistTopTracksResult")]
public abstract record RefreshArtistTopTracksResult;

public record RefreshArtistTopTracksSuccess(Artist Artist) : RefreshArtistTopTracksResult;

public record RefreshArtistTopTracksUnknownError(string Message) : RefreshArtistTopTracksResult;
