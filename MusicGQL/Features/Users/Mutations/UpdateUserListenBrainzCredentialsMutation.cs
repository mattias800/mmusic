using MusicGQL.Db.Postgres;
using MusicGQL.EventProcessor;
using MusicGQL.Db.Postgres.Models;
using MusicGQL.Features.Users.Db;
using MusicGQL.Features.Users.Events;
using MusicGQL.Features.Users.Services;
using MusicGQL.Types;
using Microsoft.EntityFrameworkCore;

namespace MusicGQL.Features.Users.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class UpdateUserListenBrainzCredentialsMutation
{
    public async Task<UpdateUserListenBrainzCredentialsResult> UpdateUserListenBrainzCredentials(
        UpdateUserListenBrainzCredentialsInput input,
        [Service] EventDbContext dbContext,
        [Service] EventProcessorWorker eventProcessor,
        [Service] UserListenBrainzService userListenBrainzService
    )
    {
        try
        {
            // Find the user
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == input.UserId);
            if (user == null)
            {
                return new UpdateUserListenBrainzCredentialsError("User not found");
            }

            // Determine what to update
            var newListenBrainzUserId = input.ListenBrainzUserId;
            var newListenBrainzToken = input.ListenBrainzToken;

            // If token is provided (not empty), validate it
            if (!string.IsNullOrEmpty(newListenBrainzToken))
            {
                // Create a temporary user with the new token for validation
                var tempUser = new DbUser
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    ListenBrainzUserId = newListenBrainzUserId ?? user.ListenBrainzUserId,
                    ListenBrainzToken = newListenBrainzToken
                };

                var validationResult = await userListenBrainzService.ValidateTokenAsync(tempUser);
                if (!validationResult.IsValid)
                {
                    return new UpdateUserListenBrainzCredentialsError($"Invalid ListenBrainz token: {validationResult.Message}");
                }

                // Update the user's ListenBrainz user ID if it's different from what we got from validation
                if (!string.IsNullOrEmpty(validationResult.User) && validationResult.User != newListenBrainzUserId)
                {
                    newListenBrainzUserId = validationResult.User;
                }
            }
            else
            {
                // If no token provided, keep the existing token
                newListenBrainzToken = user.ListenBrainzToken;
            }

            // If no user ID provided, keep the existing one
            if (string.IsNullOrEmpty(newListenBrainzUserId))
            {
                newListenBrainzUserId = user.ListenBrainzUserId;
            }

            // Create the event
            var event_ = new UserListenBrainzCredentialsUpdated(
                input.UserId,
                newListenBrainzUserId,
                newListenBrainzToken
            );

            dbContext.Events.Add(event_);
            await dbContext.SaveChangesAsync();
            await eventProcessor.ProcessEvents();

            // Get the updated user
            var updatedUser = await dbContext.Users.FindAsync(input.UserId);
            if (updatedUser == null)
            {
                return new UpdateUserListenBrainzCredentialsError("Failed to retrieve updated user");
            }

            return new UpdateUserListenBrainzCredentialsSuccess(new(updatedUser));
        }
        catch (Exception ex)
        {
            return new UpdateUserListenBrainzCredentialsError($"Failed to update ListenBrainz credentials: {ex.Message}");
        }
    }
}

public record UpdateUserListenBrainzCredentialsInput(
    Guid UserId,
    string? ListenBrainzUserId,
    string? ListenBrainzToken
);

[UnionType]
public abstract record UpdateUserListenBrainzCredentialsResult;

public record UpdateUserListenBrainzCredentialsSuccess(User User) : UpdateUserListenBrainzCredentialsResult;

public record UpdateUserListenBrainzCredentialsError(string Message) : UpdateUserListenBrainzCredentialsResult;
