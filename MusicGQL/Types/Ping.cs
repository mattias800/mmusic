namespace MusicGQL.Types;

public record Ping
{
    [ID]
    public string Id() => "Ping";
};
