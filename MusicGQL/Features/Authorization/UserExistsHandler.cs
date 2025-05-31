using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;

namespace MusicGQL.Features.Authorization;

public class UserExistsHandler(
    IDbContextFactory<EventDbContext> dbContextFactory,
    IHttpContextAccessor httpContextAccessor
) : AuthorizationHandler<UserExistsRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserExistsRequirement requirement
    )
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            context.Fail(); // HttpContext is null
            return;
        }

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is null)
        {
            context.Fail(); // User is not authenticated
            return;
        }

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
        {
            context.Fail(); // Invalid UserId format
            return;
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var user = await dbContext
            .Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user is null)
        {
            context.Fail(); // User not found in database
            return;
        }

        context.Succeed(requirement);
    }
}
