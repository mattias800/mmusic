namespace MusicGQL.Features.Authentication.Commands;

public record VerifyPasswordCommand(string Password, string HashedPassword);

public record VerifyPasswordResult(bool IsValid);
