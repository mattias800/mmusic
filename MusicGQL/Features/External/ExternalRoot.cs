using MusicGQL.Features.External.SoulSeek;

namespace MusicGQL.Features.External;

public record ExternalRoot()
{
    [ID]
    public string Id => "External";

    public SoulSeekRoot Soulseek() => new();
}
