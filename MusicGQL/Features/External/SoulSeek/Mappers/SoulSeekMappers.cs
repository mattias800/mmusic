using MusicGQL.Features.External.SoulSeek.Integration;

namespace MusicGQL.Features.External.SoulSeek.Mappers;

public static class SoulSeekMappers
{
    public static SoulSeekStatusType ToGql(this SoulSeekNetworkState s)
    {
        return s switch
        {
            SoulSeekNetworkState.Offline => SoulSeekStatusType.Offline,
            SoulSeekNetworkState.Connecting => SoulSeekStatusType.Connecting,
            SoulSeekNetworkState.Online => SoulSeekStatusType.Online,
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null),
        };
    }
}
