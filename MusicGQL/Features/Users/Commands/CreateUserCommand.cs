using MusicGQL.Db.Postgres.Models.Projections;

namespace MusicGQL.Features.Users.Commands;

public record CreateUserCommand(string Username, string Password);

// Define result types based on UserMutations payloads
public abstract record CreateUserResult;
public record CreateUserSuccess(UserProjection User) : CreateUserResult;
public record CreateUserFailure(string Message) : CreateUserResult; 