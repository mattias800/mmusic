using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.EventProcessor;
using MusicGQL.Features.Authentication.Commands;
using MusicGQL.Features.Authentication.Handlers;
using MusicGQL.Features.Users.Db;
using MusicGQL.Features.Users.Events; // Changed from Services to Handlers
using MusicGQL.Features.Users.Roles;

namespace MusicGQL.Features.Users.Handlers;

public class CreateUserHandler(
    EventDbContext dbContext,
    HashPasswordHandler hashPasswordHandler, // Inject HashPasswordHandler
    EventProcessorWorker eventProcessor
)
{
    public async Task<Result> Handle(Command command)
    {
        var existingUser = await dbContext.Users.FirstOrDefaultAsync(u =>
            u.Username == command.Username
        );

        if (existingUser != null)
        {
            return new Result.Error($"Username '{command.Username}' is already taken.");
        }

        var userId = Guid.NewGuid();

        var userCreatedEvent = new UserCreated
        {
            SubjectUserId = userId,
            Username = command.Username,
        };
        dbContext.Events.Add(userCreatedEvent);

        var hashPasswordResult = await hashPasswordHandler.Handle(
            new HashPasswordCommand(command.Password)
        );
        var passwordSetEvent = new UserPasswordHashUpdated(
            userId,
            hashPasswordResult.HashedPassword
        );
        dbContext.Events.Add(passwordSetEvent);

        await dbContext.SaveChangesAsync();
        await eventProcessor.ProcessEvents();

        var newUserProjection = await dbContext.Users.FindAsync(userId);
        if (newUserProjection == null)
        {
            return new Result.Error(
                "Failed to create user. Projection not found after event processing."
            );
        }

        // Initialize basic roles for first user as Admin if this is the first user in system
        var isFirstUser = !await dbContext.Users.AnyAsync(u => u.UserId != userId);
        if (isFirstUser)
        {
            dbContext.Events.Add(new UserRolesUpdated
            {
                SubjectUserId = userId,
                Roles = UserRoles.Admin
            });
            await dbContext.SaveChangesAsync();
            await eventProcessor.ProcessEvents();
        }

        return new Result.Success(newUserProjection);
    }

    public record Command(string Username, string Password);

    public abstract record Result
    {
        public record Success(DbUser DbUser) : Result;

        public record Error(string Message) : Result;
    }
}
