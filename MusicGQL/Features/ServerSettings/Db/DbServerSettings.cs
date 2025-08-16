namespace MusicGQL.Features.ServerSettings.Db;

public class DbServerSettings
{
    public int Id { get; set; }

    public string LibraryPath { get; set; } = string.Empty;
    public string DownloadPath { get; set; } = string.Empty;

    // Max time (seconds) a SoulSeek search may run before yielding to next queued job.
    // When queue has more items, an ongoing search exceeding this will be aborted and re-queued to the back.
    public int SoulSeekSearchTimeLimitSeconds { get; set; } = 60;

    // Max seconds to wait without receiving data during a SoulSeek transfer before cancelling it
    public int SoulSeekNoDataTimeoutSeconds { get; set; } = 20;

    // Number of concurrent download slots to use
    public int DownloadSlotCount { get; set; } = 3;

    // ListenBrainz integration settings
    public string ListenBrainzUsername { get; set; } = string.Empty;
    public string ListenBrainzApiKey { get; set; } = string.Empty;
}

public static class DefaultDbServerSettingsProvider
{
    public const int ServerSettingsSingletonId = 10;

    public static DbServerSettings GetDefault()
    {
        return new()
        {
            Id = ServerSettingsSingletonId,
            LibraryPath = "",
            DownloadPath = "",
            SoulSeekSearchTimeLimitSeconds = 60,
            SoulSeekNoDataTimeoutSeconds = 20,
            DownloadSlotCount = 3,
            ListenBrainzUsername = "",
            ListenBrainzApiKey = "",
        };
    }
}
