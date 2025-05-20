using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models.Projections;
using MusicGQL.Features.Authentication.Commands;
using MusicGQL.Features.Authentication.Handlers;
using MusicGQL.Features.Users.Commands;
using MusicGQL.Features.Users.Handlers;
using MusicGQL.Types;

namespace MusicGQL.Features.Users.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class CreateUserMutation
{
    public async Task<object> CreateUser(
        [Service] CreateUserHandler createUserHandler,
        CreateUserInput input
    )
    {
        var result = await createUserHandler.Handle(
            new CreateUserCommand(input.Username, input.Password)
        );

        return result switch
        {
            CreateUserSuccess success => new CreateUserPayload(success.User),
            CreateUserFailure failure => new CreateUserErrorPayload(failure.Message),
            _ => new CreateUserErrorPayload("An unexpected error occurred."),
        };
    }

    public async Task<object> Login(
        LoginInput input,
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
            return new LoginErrorPayload("Invalid username or password.");
        }

        var verifyResult = await verifyPasswordHandler.Handle(
            new VerifyPasswordCommand(input.Password, user.PasswordHash)
        );

        if (!verifyResult.IsValid)
        {
            return new LoginErrorPayload("Invalid username or password.");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
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

        return new LoginSuccessPayload(user, "Login successful.");
    }
}

public record CreateUserInput(string Username, string Password);

public record CreateUserPayload(UserProjection User);

public record CreateUserErrorPayload(string Message);

public record LoginInput(string Username, string Password);

public record LoginSuccessPayload(UserProjection User, string Message);

public record LoginErrorPayload(string Message);
