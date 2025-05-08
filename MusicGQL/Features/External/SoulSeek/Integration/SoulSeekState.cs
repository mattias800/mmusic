namespace MusicGQL.Features.External.SoulSeek.Integration;

public enum SoulSeekNetworkState
{
    Offline,
    Connecting,
    Online,
}

public record SoulSeekState(SoulSeekNetworkState NetworkState);
