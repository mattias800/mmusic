using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;

namespace MusicGQL.Features.Users;

public record UserSearchRoot
{
    [UsePaging]
    public async Task<IEnumerable<User>> GetUsers(
        [Service] EventDbContext dbContext,
        ClaimsPrincipal claims
    )
    {
        // Only Admins can list users
        var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null) return [];
        var userId = Guid.Parse(userIdClaim.Value);
        var viewer = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
        if (viewer is null || (viewer.Roles & Users.Roles.UserRoles.Admin) == 0) return [];

        var projections = await dbContext.Users.ToListAsync();
        return projections.Select(p => new User(p));
    }
}
