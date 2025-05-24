namespace MusicGQL.Features.Authentication.Commands;

public record HashPasswordCommand(string Password);

public record HashPasswordResult(string HashedPassword);
