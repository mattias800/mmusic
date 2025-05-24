using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;

namespace MusicGQL.Features.Users;

public record UserSearchRoot
{
    [UsePaging]
    public async Task<IEnumerable<User>> GetUsers([Service] EventDbContext dbContext)
    {
        var projections = await dbContext.UserProjections.ToListAsync();
        return projections.Select(p => new User(p));
    }

    // Optional: Query to get the current logged-in user
    // This would require HttpContextAccessor and checking User.Identity
    /*
    public UserProjection? GetCurrentUser([Service] IHttpContextAccessor httpContextAccessor)
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return null;
        }

        // This would typically fetch the user from UserProjections using the userId
        // For simplicity, returning a placeholder or requiring another service call.
        // Ideally, you'd inject EventDbContext here and fetch.
        // This is just a conceptual placeholder.
        // var user = await dbContext.UserProjections.FindAsync(userId);
        // return user;
        return new UserProjection { UserId = userId, Username = httpContextAccessor.HttpContext.User.Identity.Name };
    }
    */
}
