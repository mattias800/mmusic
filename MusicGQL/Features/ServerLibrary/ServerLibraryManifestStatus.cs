using MusicGQL.Features.ServerSettings;

namespace MusicGQL.Features.ServerLibrary;

public record ServerLibraryManifestStatus
{
    [ID]
    public string Id() => "Manifest";

    public async Task<bool> HasLibraryManifest(
        ServerSettingsAccessor serverSettingsAccessor,
        LibraryManifestService manifestService
    )
    {
        try
        {
            var settings = await serverSettingsAccessor.GetAsync();
            var path = settings.LibraryPath;
            return await manifestService.HasManifestAsync(path);
        }
        catch
        {
            return false;
        }
    }
}
