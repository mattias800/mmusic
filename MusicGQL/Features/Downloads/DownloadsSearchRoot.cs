using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Downloads.Services;

namespace MusicGQL.Features.Downloads;

public class DownloadsSearchRoot
{
    public DownloadQueueState DownloadQueue(
        ClaimsPrincipal claims,
        [Service] EventDbContext dbContext,
        DownloadQueueService queue
    )
    {
        var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null)
            return new DownloadQueueState { QueueLength = 0, Items = [] };
        var userId = Guid.Parse(userIdClaim.Value);
        var user = dbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserId == userId);
        var canView =
            user is not null
            && (user.Roles & (Users.Roles.UserRoles.ViewDownloads | Users.Roles.UserRoles.Admin))
                != 0;
        return canView ? queue.Snapshot() : new DownloadQueueState { QueueLength = 0, Items = [] };
    }

    public List<DownloadHistoryItem> DownloadHistory(
        ClaimsPrincipal claims,
        [Service] EventDbContext dbContext,
        DownloadHistoryService history
    )
    {
        var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null)
            return [];
        var userId = Guid.Parse(userIdClaim.Value);
        var user = dbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserId == userId);
        var canView =
            user is not null
            && (user.Roles & (Users.Roles.UserRoles.ViewDownloads | Users.Roles.UserRoles.Admin))
                != 0;
        return canView ? history.List() : [];
    }

    // New multi-slot queries
    public IReadOnlyDictionary<int, DownloadProgress> AllSlotProgress(
        ClaimsPrincipal claims,
        [Service] EventDbContext dbContext,
        CurrentDownloadStateService state
    )
    {
        var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null)
            return new Dictionary<int, DownloadProgress>();
        var userId = Guid.Parse(userIdClaim.Value);
        var user = dbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserId == userId);
        var canView =
            user is not null
            && (user.Roles & (Users.Roles.UserRoles.ViewDownloads | Users.Roles.UserRoles.Admin))
                != 0;
        return canView ? state.GetAllSlotProgress() : new Dictionary<int, DownloadProgress>();
    }

    public DownloadProgress? SlotProgress(
        ClaimsPrincipal claims,
        [Service] EventDbContext dbContext,
        CurrentDownloadStateService state,
        int slotId
    )
    {
        var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null)
            return null;
        var userId = Guid.Parse(userIdClaim.Value);
        var user = dbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserId == userId);
        var canView =
            user is not null
            && (user.Roles & (Users.Roles.UserRoles.ViewDownloads | Users.Roles.UserRoles.Admin))
                != 0;
        return canView ? state.GetSlotProgress(slotId) : null;
    }

    public List<DownloadSlotInfo> DownloadSlots(
        ClaimsPrincipal claims,
        [Service] EventDbContext dbContext,
        IDownloadSlotManager slotManager
    )
    {
        var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null)
            return [];
        var userId = Guid.Parse(userIdClaim.Value);
        var user = dbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserId == userId);
        var canView =
            user is not null
            && (user.Roles & (Users.Roles.UserRoles.ViewDownloads | Users.Roles.UserRoles.Admin))
                != 0;
        return canView ? slotManager.GetSlotsInfo() : [];
    }
}
