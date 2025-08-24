using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Users.Handlers;
using MusicGQL.Types;

namespace MusicGQL.Features.Users.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class CreateUserMutation
{
    public async Task<CreateUserResult> CreateUser(
        [Service] CreateUserHandler createUserHandler,
        CreateUserInput input,
        [Service] EventDbContext dbContext
    )
    {
        var result = await createUserHandler.Handle(new(input.Username, input.Password));

        return result switch
        {
            CreateUserHandler.Result.Success success => new CreateUserSuccess(
                new(success.DbUser),
                (await dbContext.Users.ToListAsync()).Select(u => new User(u))
            ),
            CreateUserHandler.Result.Error failure => new CreateUserError(failure.Message),
            _ => new CreateUserError("An unexpected error occurred."),
        };
    }
}

public record CreateUserInput(string Username, string Password);

[UnionType]
public abstract record CreateUserResult;

public record CreateUserSuccess(User User, IEnumerable<User> Users) : CreateUserResult;

public record CreateUserError(string Message) : CreateUserResult;
