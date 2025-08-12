using MusicGQL.Features.ServerSettings.Db;

namespace MusicGQL.Features.ServerSettings;

public record ServerSettings([property: GraphQLIgnore] DbServerSettings Model)
{
    [ID]
    public int Id() => Model.Id;

    public string LibraryPath() => Model.LibraryPath;

    public string DownloadPath() => Model.DownloadPath;

    public int SoulSeekSearchTimeLimitSeconds() => Model.SoulSeekSearchTimeLimitSeconds;
}
