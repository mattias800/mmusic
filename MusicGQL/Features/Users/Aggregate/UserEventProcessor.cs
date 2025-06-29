using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models;
using MusicGQL.Features.Users.Db;
using MusicGQL.Features.Users.Events;

namespace MusicGQL.Features.Users.Aggregate;

public class UserEventProcessor(ILogger<UserEventProcessor> logger)
{
    public async Task ProcessEvent(Event ev, EventDbContext dbContext)
    {
        switch (ev)
        {
            case UserCreated userCreated:
                await HandleUserCreated(userCreated, dbContext);
                break;
            case UserPasswordHashUpdated userPasswordHashSet:
                await HandleUserPasswordHashSet(userPasswordHashSet, dbContext);
                break;
        }
    }

    private async Task HandleUserCreated(UserCreated ev, EventDbContext dbContext)
    {
        var existing = dbContext.Users.FirstOrDefault(u => u.UserId == ev.SubjectUserId);

        if (existing is not null)
        {
            logger.LogWarning(
                "UserCreated event received for UserId: {UserId}, but user with that id already exists. Ignoring",
                ev.SubjectUserId
            );
            return;
        }

        dbContext.Users.Add(
            new DbUser
            {
                UserId = ev.SubjectUserId,
                CreatedAt = ev.CreatedAt,
                Username = ev.Username,
            }
        );

        await dbContext.SaveChangesAsync();
    }

    private async Task HandleUserPasswordHashSet(
        UserPasswordHashUpdated ev,
        EventDbContext dbContext
    )
    {
        var user = dbContext.Users.FirstOrDefault(u => u.UserId == ev.SubjectUserId);

        if (user is null)
        {
            logger.LogWarning(
                "UserPasswordHashSet event received for UserId: {UserId}, but no existing user found. Ignoring",
                ev.SubjectUserId
            );
            return;
        }

        user.PasswordHash = ev.PasswordHash;
        user.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
    }
}
