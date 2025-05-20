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
        var result = await createUserHandler.Handle(new(input.Username, input.Password));

        return result switch
        {
            CreateUserHandler.Result.Success success => new CreateUserResult.Success(
                new(success.User)
            ),
            CreateUserHandler.Result.Error failure => new CreateUserResult.Error(failure.Message),
            _ => new CreateUserResult.Error("An unexpected error occurred."),
        };
    }
}

public record CreateUserInput(string Username, string Password);

[UnionType]
public abstract record CreateUserResult
{
    public record Success(User User) : CreateUserResult;

    public record Error(string Message) : CreateUserResult;
}
