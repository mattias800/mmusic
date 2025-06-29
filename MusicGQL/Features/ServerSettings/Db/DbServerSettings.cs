namespace MusicGQL.Features.ServerSettings.Db;

public class DbServerSettings
{
    public int Id { get; set; }

    public string LibraryPath { get; set; } = string.Empty;
}

public static class DefaultDbServerSettingsProvider
{
    public const int ServerSettingsSingletonId = 10;

    public static DbServerSettings GetDefault()
    {
        return new() { Id = ServerSettingsSingletonId, LibraryPath = "" };
    }
}
