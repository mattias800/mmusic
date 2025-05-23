using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Downloads;
using MusicGQL.Features.External;
using MusicGQL.Features.MusicBrainz;
using MusicGQL.Features.Playlists;
using MusicGQL.Features.Recommendations;
using MusicGQL.Features.ServerLibrary.Artist;
using MusicGQL.Features.ServerLibrary.Release;
using MusicGQL.Features.ServerLibrary.ReleaseGroup;
using MusicGQL.Features.Users;

namespace MusicGQL.Types;

public class Query
{
    public ArtistSearchRoot Artist => new();
    public ReleaseGroupSearchRoot ReleaseGroup => new();
    public ReleaseSearchRoot Release => new();

    public MusicBrainzSearchRoot MusicBrainz => new();

    // Implemented Viewer field
    // This method will be resolved by HotChocolate as the 'viewer' field in the GraphQL schema.
    public async Task<User?> GetViewer(
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] EventDbContext dbContext
    )
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            // This state is unexpected in a typical request pipeline.
            // Log.Error("HttpContext is null in GetViewer resolver.");
            return null;
        }

        if (httpContext.User.Identity == null || !httpContext.User.Identity.IsAuthenticated)
        {
            return null; // User is not authenticated
        }

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            // User is authenticated, but the NameIdentifier claim is missing or invalid.
            // Log.Warning("Authenticated user has missing or invalid NameIdentifier claim.");
            return null;
        }

        var userProjection = await dbContext.UserProjections.FirstOrDefaultAsync(up =>
            up.UserId == userId
        );

        if (userProjection == null)
        {
            // A user claim was present, but no corresponding user projection was found in the database.
            // This could happen if the user was deleted after the cookie was issued.
            // Log.Warning($"User projection not found for authenticated user ID: {userId}");
            return null;
        }

        return new User(userProjection);
    }

    public DownloadsSearchRoot Download => new();
    public ExternalRoot External => new();
    public PlaylistSearchRoot Playlist => new();
    public RecommendationsSearchRoot Recommendations => new();
    public UserSearchRoot User => new(); // This is for general user queries via UserSearchRoot

    // New query to check if any users exist
    public async Task<bool> GetAreThereAnyUsers([Service] EventDbContext dbContext)
    {
        return await dbContext.UserProjections.AnyAsync();
    }
}
