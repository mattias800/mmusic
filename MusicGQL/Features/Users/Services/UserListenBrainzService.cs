using MetaBrainz.ListenBrainz;
using MetaBrainz.ListenBrainz.Interfaces;
using MetaBrainz.ListenBrainz.Objects;
using Microsoft.Extensions.Logging;
using MusicGQL.Features.Users.Db;

namespace MusicGQL.Features.Users.Services;

public class UserListenBrainzService(ILogger<UserListenBrainzService> logger)
{
    /// <summary>
    /// Creates a ListenBrainz client for a specific user
    /// </summary>
    private MetaBrainz.ListenBrainz.ListenBrainz CreateClientForUser(DbUser user)
    {
        if (string.IsNullOrEmpty(user.ListenBrainzToken))
        {
            throw new InvalidOperationException($"User {user.Username} does not have ListenBrainz credentials configured");
        }

        // Create a new client instance for this user
        var client = new MetaBrainz.ListenBrainz.ListenBrainz("MusicGQL", "1.0", "mmusic@example.com");
        client.UserToken = user.ListenBrainzToken;
        
        return client;
    }

    /// <summary>
    /// Submits a single listen for a specific user
    /// </summary>
    public async Task<bool> SubmitSingleListenAsync(DbUser user, ISubmittedListen listen)
    {
        try
        {
            var client = CreateClientForUser(user);
            await client.SubmitSingleListenAsync(listen);
            
            logger.LogInformation("Successfully submitted listen to ListenBrainz for user {Username}", user.Username);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to submit listen to ListenBrainz for user {Username}", user.Username);
            return false;
        }
    }

    /// <summary>
    /// Submits multiple listens for a specific user
    /// </summary>
    public async Task<bool> SubmitListensAsync(DbUser user, IEnumerable<ISubmittedListen> listens)
    {
        try
        {
            var client = CreateClientForUser(user);
            await client.ImportListensAsync(listens);
            
            logger.LogInformation("Successfully submitted {Count} listens to ListenBrainz for user {Username}", 
                listens.Count(), user.Username);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to submit {Count} listens to ListenBrainz for user {Username}", 
                listens.Count(), user.Username);
            return false;
        }
    }

    /// <summary>
    /// Validates a ListenBrainz token for a user
    /// </summary>
    public async Task<(bool IsValid, string? User, string? Message)> ValidateTokenAsync(DbUser user, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(user.ListenBrainzToken))
            {
                return (false, null, "No ListenBrainz token configured for user");
            }

            var client = CreateClientForUser(user);
            var result = await client.ValidateTokenAsync(user.ListenBrainzToken, cancellationToken);
            
            // Valid can be null -> fall back to message when needed
            var isValid = result.Valid ?? string.Equals(result.Message, "ok", StringComparison.OrdinalIgnoreCase);
            return (isValid, result.User, result.Message);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to validate ListenBrainz token for user {Username}", user.Username);
            return (false, null, ex.Message);
        }
    }
}
