using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Authentication.Commands;
using MusicGQL.Features.Authentication.Handlers;
using MusicGQL.Features.Authentication.Services;
using MusicGQL.Types;

namespace MusicGQL.Features.Users.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class SignInMutation
{
    public async Task<SignInResult> SignIn(
        SignInInput input,
        [Service] EventDbContext dbContext,
        [Service] VerifyPasswordHandler verifyPasswordHandler,
        [Service] JwtTokenService jwtTokenService,
        [Service] IHttpContextAccessor httpContextAccessor
    )
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == input.Username);

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
        {
            return new SignInError("Invalid username or password.");
        }

        var verifyResult = await verifyPasswordHandler.Handle(
            new VerifyPasswordCommand(input.Password, user.PasswordHash)
        );

        if (!verifyResult.IsValid)
        {
            return new SignInError("Invalid username or password.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        };

        var claimsIdentity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        var authProperties = new AuthenticationProperties { };

        // Still set cookie for web compatibility
        await httpContextAccessor.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties
        );

        // Generate JWT token for mobile/desktop apps
        var token = jwtTokenService.GenerateToken(user.UserId, user.Username);

        return new SignInSuccess(new(user), token);
    }
}

public record SignInInput(string Username, string Password);

[UnionType]
public abstract record SignInResult;

public record SignInSuccess(User User, string Token) : SignInResult;

public record SignInError(string Message) : SignInResult;
