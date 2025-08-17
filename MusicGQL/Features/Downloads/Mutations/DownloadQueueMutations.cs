using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;

namespace MusicGQL.Features.Downloads.Mutations;

[ExtendObjectType(typeof(Types.Mutation))]
public sealed class DownloadQueueMutations
{
    public bool RemoveDownloadJob(
        string queueKey,
        [Service] Services.DownloadQueueService queue,
        ClaimsPrincipal claims,
        [Service] EventDbContext dbContext
    )
    {
        var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null) return false;
        var userId = Guid.Parse(userIdClaim.Value);
        var user = dbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserId == userId);
        var canManage = user is not null && (user.Roles & (Features.Users.Roles.UserRoles.TriggerDownloads | Features.Users.Roles.UserRoles.Admin)) != 0;
        if (!canManage) return false;
        return queue.TryRemove(queueKey);
    }
}


