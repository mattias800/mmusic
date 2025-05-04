namespace MusicGQL.Types;

public class Mutation
{
    public Task<bool> Ping() => Task.FromResult(true);
}
