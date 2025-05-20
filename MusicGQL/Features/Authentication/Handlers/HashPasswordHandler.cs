using MusicGQL.Features.Authentication.Commands;
using BCryptNet = BCrypt.Net.BCrypt;

namespace MusicGQL.Features.Authentication.Handlers;

public class HashPasswordHandler
{
    public Task<HashPasswordResult> Handle(HashPasswordCommand command)
    {
        var hashedPassword = BCryptNet.HashPassword(command.Password);
        return Task.FromResult(new HashPasswordResult(hashedPassword));
    }
}
