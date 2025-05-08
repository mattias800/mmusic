namespace MusicGQL.Features.External.SoulSeek;

public enum SoulSeekStatusType
{
    Offline,
    Connecting,
    Online,
}

public record SoulSeekStatus
{
    [ID]
    public string Id() => "SoulSeekStatus";

    public SoulSeekStatusType Status() => SoulSeekStatusType.Offline;
}
