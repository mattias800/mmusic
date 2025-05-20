using System; // For Guid
using System.Collections.Generic; // For Dictionary
using System.Linq; // For ToDictionaryAsync
using System.Threading.Tasks; // For Task
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // For ILogger
using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models;
using MusicGQL.Db.Postgres.Models.Events.Users;
using MusicGQL.Db.Postgres.Models.Projections;

namespace MusicGQL.Features.Users.Aggregate; // Changed namespace

public class UserEventProcessor(ILogger<UserEventProcessor> logger)
{
    private Dictionary<Guid, UserProjection> _userProjectionsCache = new();

    public async Task PrepareProcessing(EventDbContext dbContext)
    {
        logger.LogInformation("Initializing UserEventProcessor cache...");
        _userProjectionsCache = await dbContext.UserProjections.ToDictionaryAsync(p => p.UserId);
        logger.LogInformation(
            "UserEventProcessor cache initialized with {Count} users.",
            _userProjectionsCache.Count
        );
    }

    public void ProcessEvent(Event ev, EventDbContext dbContext)
    {
        switch (ev)
        {
            case UserCreated userCreated:
                HandleUserCreated(userCreated, dbContext);
                break;
            case UserPasswordHashSet userPasswordHashSet:
                HandleUserPasswordHashSet(userPasswordHashSet, dbContext);
                break;
        }
    }

    private void HandleUserCreated(UserCreated userCreated, EventDbContext dbContext)
    {
        if (!_userProjectionsCache.TryGetValue(userCreated.SubjectUserId, out var projection))
        {
            projection = new UserProjection
            {
                UserId = userCreated.SubjectUserId,
                CreatedAt = userCreated.CreatedAt,
                UpdatedAt = userCreated.CreatedAt,
            };
            dbContext.UserProjections.Add(projection);
            _userProjectionsCache[userCreated.SubjectUserId] = projection;
            logger.LogInformation(
                "User projection created for UserId: {UserId}",
                userCreated.SubjectUserId
            );
        }

        projection.Username = userCreated.Username;
        projection.UpdatedAt = userCreated.CreatedAt;
    }

    private void HandleUserPasswordHashSet(
        UserPasswordHashSet userPasswordHashSet,
        EventDbContext dbContext
    )
    {
        if (
            _userProjectionsCache.TryGetValue(userPasswordHashSet.SubjectUserId, out var projection)
        )
        {
            projection.PasswordHash = userPasswordHashSet.PasswordHash;
            projection.UpdatedAt = userPasswordHashSet.CreatedAt;
            logger.LogInformation(
                "Password hash set for UserId: {UserId}",
                userPasswordHashSet.SubjectUserId
            );
        }
        else
        {
            logger.LogWarning(
                "UserPasswordHashSet event received for UserId: {UserId}, but no existing projection found. Creating one",
                userPasswordHashSet.SubjectUserId
            );
            projection = new UserProjection
            {
                UserId = userPasswordHashSet.SubjectUserId,
                PasswordHash = userPasswordHashSet.PasswordHash,
                CreatedAt = userPasswordHashSet.CreatedAt,
                UpdatedAt = userPasswordHashSet.CreatedAt,
            };
            dbContext.UserProjections.Add(projection);
            _userProjectionsCache[userPasswordHashSet.SubjectUserId] = projection;
        }
    }
} 