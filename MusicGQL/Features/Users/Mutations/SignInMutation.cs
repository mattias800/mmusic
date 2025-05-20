using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Authentication.Commands;
using MusicGQL.Features.Authentication.Handlers;
using MusicGQL.Types;

namespace MusicGQL.Features.Users.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class SignInMutation
{
    public async Task<object> SignIn(
        SignInInput input,
        [Service] EventDbContext dbContext,
        [Service] VerifyPasswordHandler verifyPasswordHandler,
        [Service] IHttpContextAccessor httpContextAccessor
    )
    {
        var user = await dbContext.UserProjections.FirstOrDefaultAsync(u =>
            u.Username == input.Username
        );

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
        {
            return new SignInResult.Error("Invalid username or password.");
        }

        var verifyResult = await verifyPasswordHandler.Handle(
            new VerifyPasswordCommand(input.Password, user.PasswordHash)
        );

        if (!verifyResult.IsValid)
        {
            return new SignInResult.Error("Invalid username or password.");
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

        await httpContextAccessor.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties
        );

        return new SignInResult.Success(new(user));
    }
}

public record SignInInput(string Username, string Password);

[UnionType]
public abstract record SignInResult
{
    public record Success(User User) : SignInResult;

    public record Error(string Message) : SignInResult;
}
