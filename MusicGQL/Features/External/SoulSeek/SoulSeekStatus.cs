using MusicGQL.Features.External.SoulSeek.Integration;
using MusicGQL.Features.External.SoulSeek.Mappers;

namespace MusicGQL.Features.External.SoulSeek;

public enum SoulSeekStatusType
{
    Offline,
    Connecting,
    Online,
}

public record SoulSeekStatus([property: GraphQLIgnore] SoulSeekState Model)
{
    [ID]
    public string Id() => "SoulSeekStatus";

    public SoulSeekStatusType Status() => Model.NetworkState.ToGql();
}
