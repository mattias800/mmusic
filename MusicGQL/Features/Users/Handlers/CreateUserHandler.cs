using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models.Events.Users;
using MusicGQL.EventProcessor;
using MusicGQL.Features.Authentication.Commands;
using MusicGQL.Features.Authentication.Handlers; // Changed from Services to Handlers
using MusicGQL.Features.Users.Commands;

namespace MusicGQL.Features.Users.Handlers;

public class CreateUserHandler(
    EventDbContext dbContext,
    HashPasswordHandler hashPasswordHandler, // Inject HashPasswordHandler
    EventProcessorWorker eventProcessor
)
{
    public async Task<CreateUserResult> Handle(CreateUserCommand command)
    {
        var existingUser = await dbContext.UserProjections.FirstOrDefaultAsync(u =>
            u.Username == command.Username
        );

        if (existingUser != null)
        {
            return new CreateUserFailure($"Username '{command.Username}' is already taken.");
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
        var passwordSetEvent = new UserPasswordHashSet(userId, hashPasswordResult.HashedPassword);
        dbContext.Events.Add(passwordSetEvent);

        await dbContext.SaveChangesAsync();
        await eventProcessor.ProcessEvents();

        var newUserProjection = await dbContext.UserProjections.FindAsync(userId);
        if (newUserProjection == null)
        {
            return new CreateUserFailure(
                "Failed to create user. Projection not found after event processing."
            );
        }

        return new CreateUserSuccess(newUserProjection);
    }
}
