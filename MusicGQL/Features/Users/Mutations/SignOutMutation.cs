using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MusicGQL.Types; // Assuming Mutation type is accessible here

namespace MusicGQL.Features.Users.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class SignOutMutation
{
    public async Task<SignOutResult> SignOut(
        [Service] IHttpContextAccessor httpContextAccessor
    )
    {
        try
        {
            if (httpContextAccessor.HttpContext == null)
            {
                return new SignOutError("HttpContext is not available.");
            }

            await httpContextAccessor.HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );
            return new SignOutSuccess(true);
        }
        catch (Exception ex)
        {
            // It's good practice to log the exception here
            // For example: _logger.LogError(ex, "Error during sign out");
            return new SignOutError($"An error occurred during sign out: {ex.Message}");
        }
    }
}

[UnionType]
public abstract record SignOutResult;

public record SignOutSuccess(bool Success) : SignOutResult;

public record SignOutError(string Message) : SignOutResult; 