using MusicGQL.Features.External.SoulSeek.Integration;

namespace MusicGQL.Features.External.SoulSeek;

public record SoulSeekRoot
{
    [ID]
    public string Id => "SoulSeekRoot";

    public SoulSeekStatus Status([Service] SoulSeekService soulSeekService) =>
        new(soulSeekService.State);
};
