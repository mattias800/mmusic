using MusicGQL.Features.Authentication.Commands;
using BCryptNet = BCrypt.Net.BCrypt;

namespace MusicGQL.Features.Authentication.Handlers;

public class VerifyPasswordHandler
{
    public Task<VerifyPasswordResult> Handle(VerifyPasswordCommand command)
    {
        var isValid = BCryptNet.Verify(command.Password, command.HashedPassword);
        return Task.FromResult(new VerifyPasswordResult(isValid));
    }
}
